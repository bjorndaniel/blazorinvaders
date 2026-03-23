using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace BlazorInvaders.Api;

public class GetLeaderboard
{
    [Function("getleaderboard")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
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
                filter: "PartitionKey eq 'HighScore'"))
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
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(ex.Message);
            return response;
        }
    }
}
