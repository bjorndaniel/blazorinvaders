using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace BlazorInvaders.Api;

public class GetLeaderboard(ILogger<GetLeaderboard> logger)
{
    [Function("getleaderboard")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        // ?mode=daily[&day=yyyyMMdd] for a daily challenge board; all-time otherwise
        if (!HighScoreValidator.TryGetLeaderboardPartition(req.Query["mode"], req.Query["day"], out var partition, out var queryError))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync(queryError);
            return bad;
        }

        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        if (string.IsNullOrEmpty(connectionString))
        {
            var err = req.CreateResponse(HttpStatusCode.InternalServerError);
            await err.WriteStringAsync("Storage not configured");
            return err;
        }

        try
        {
            var serviceClient = new TableServiceClient(connectionString);
            await serviceClient.CreateTableIfNotExistsAsync("HighScores");
            var tableClient = new TableClient(connectionString, "HighScores");

            var entries = new List<(string Name, int Score)>();
            await foreach (var entity in tableClient.QueryAsync<TableEntity>(
                filter: TableClient.CreateQueryFilter($"PartitionKey eq {partition}")))
            {
                var name = entity.GetString("Name") ?? "";
                var score = entity.GetInt32("Score") ?? 0;
                entries.Add((name, score));
            }

            var top10 = entries
                .OrderByDescending(e => e.Score)
                .Take(10)
                .Select(e => new { name = e.Name, score = e.Score });

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(top10));
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load leaderboard");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Could not load leaderboard");
            return response;
        }
    }
}
