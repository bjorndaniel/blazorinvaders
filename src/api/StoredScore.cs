namespace BlazorInvaders.Api;

/// <summary>One leaderboard row as read from storage.</summary>
public sealed record StoredScore(string PartitionKey, string RowKey, string Name, int Score);
