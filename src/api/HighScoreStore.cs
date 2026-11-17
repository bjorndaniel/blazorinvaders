using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace BlazorInvaders.Api;

/// <summary>Table Storage access for the leaderboard, shared by all functions of an instance.</summary>
public sealed class HighScoreStore
{
    private static readonly string[] Select = ["PartitionKey", "RowKey", "Name", "Score"];

    private readonly ILogger<HighScoreStore> _logger;
    private readonly TableClient? _table;
    private readonly LegacyTopCache _legacy;
    private readonly SemaphoreSlim _tableLock = new(1, 1);
    private bool _tableReady;

    public HighScoreStore(ILogger<HighScoreStore> logger, TimeProvider time)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable(FunctionResponses.StorageSetting);
        if (!string.IsNullOrEmpty(connectionString))
        {
            try
            {
                _table = new TableClient(connectionString, FunctionResponses.TableName);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException)
            {
                // Leave unconfigured: requests get the usual "Storage not configured" 500
                _logger.LogError(ex, "Invalid storage connection string");
            }
        }
        _legacy = new LegacyTopCache(LoadLegacyAsync, time,
            (partition, ex) => _logger.LogWarning(ex, "Could not read legacy scores of {Partition}", partition));
    }

    public bool IsConfigured => _table is not null;

    private TableClient Table => _table ?? throw new InvalidOperationException("Storage not configured");

    public async Task AddAsync(HighScoreRequest score)
    {
        await EnsureTableAsync();
        var entity = new TableEntity(
            HighScoreValidator.PartitionFor(score),
            LeaderboardRanking.RankedRowKey(score.Score, Guid.NewGuid().ToString()))
        {
            ["Name"] = score.Name,
            ["Score"] = score.Score
        };
        await Table.AddEntityAsync(entity);
    }

    public async Task<IReadOnlyList<(string Name, int Score)>> GetTopAsync(string partition, CancellationToken ct)
    {
        await EnsureTableAsync(ct);
        var legacyTop = await _legacy.GetAsync(partition, ct);

        var rankedPage = Table.QueryAsync<TableEntity>(
            filter: TableClient.CreateQueryFilter(
                $"PartitionKey eq {partition} and RowKey ge {LeaderboardRanking.RangeStart} and RowKey lt {LeaderboardRanking.RangeEnd}"),
            maxPerPage: LeaderboardRanking.MaxEntries,
            select: Select,
            cancellationToken: ct);
        var rankedEntities = await LeaderboardRanking.TakeTopAsync(rankedPage, ct);
        var rankedTop = Map(rankedEntities, partition);

        return LeaderboardRanking.Merge(legacyTop, rankedTop)
            .Select(s => (s.Name, s.Score))
            .ToList();
    }

    private async Task EnsureTableAsync(CancellationToken ct = default)
    {
        if (_tableReady)
            return;

        await _tableLock.WaitAsync(ct);
        try
        {
            if (_tableReady)
                return;
            await Table.CreateIfNotExistsAsync(ct);
            _tableReady = true;
        }
        finally
        {
            _tableLock.Release();
        }
    }

    private async Task<IReadOnlyList<StoredScore>?> LoadLegacyAsync(string partition, CancellationToken ct)
    {
        try
        {
            var top = new List<StoredScore>();
            var skipped = 0;
            foreach (var filter in new[]
            {
                TableClient.CreateQueryFilter($"PartitionKey eq {partition} and RowKey lt {LeaderboardRanking.RangeStart}"),
                TableClient.CreateQueryFilter($"PartitionKey eq {partition} and RowKey ge {LeaderboardRanking.RangeEnd}")
            })
            {
                await foreach (var entity in Table.QueryAsync<TableEntity>(filter: filter, select: Select, cancellationToken: ct))
                {
                    var row = ToStoredScore(entity);
                    if (row is null)
                    {
                        skipped++;
                        continue;
                    }
                    top = [.. LeaderboardRanking.Merge(top, [row])];
                }
            }

            if (skipped > 0)
                _logger.LogWarning("Skipped {Count} unreadable legacy scores in {Partition}", skipped, partition);
            return top;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(ex, "Could not read legacy scores of {Partition}", partition);
            return null;
        }
    }

    private List<StoredScore> Map(IEnumerable<TableEntity> entities, string partition)
    {
        var rows = new List<StoredScore>();
        var skipped = 0;
        foreach (var entity in entities)
        {
            var row = ToStoredScore(entity);
            if (row is null)
                skipped++;
            else
                rows.Add(row);
        }

        if (skipped > 0)
            _logger.LogWarning("Skipped {Count} unreadable scores in {Partition}", skipped, partition);
        return rows;
    }

    private static StoredScore? ToStoredScore(TableEntity entity)
    {
        entity.TryGetValue("Score", out var raw);
        entity.TryGetValue("Name", out var name);
        if (!LeaderboardRanking.TryReadScore(raw, out var score))
            return null;
        return new StoredScore(entity.PartitionKey, entity.RowKey, name as string ?? "", score);
    }
}
