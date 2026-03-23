using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace BlazorInvaders.Api;

public record HighScoreRequest(string Name, int Score);

public class SaveHighScore
{
    [Function("savehighscore")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        HighScoreRequest? dto;
        try
        {
            dto = await JsonSerializer.DeserializeAsync<HighScoreRequest>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            dto = null;
        }

        if (dto is null || string.IsNullOrEmpty(dto.Name))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing name or score");
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
            var entity = new TableEntity("HighScore", "Current")
            {
                ["Name"] = dto.Name,
                ["Score"] = dto.Score
            };
            await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Saved high score");
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
