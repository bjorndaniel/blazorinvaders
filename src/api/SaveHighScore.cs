using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace BlazorInvaders.Api;

public class SaveHighScore(ILogger<SaveHighScore> logger)
{
    [Function("savehighscore")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var body = await HighScoreValidator.ReadBoundedAsync(req.Body, HighScoreValidator.MaxBodyBytes);
        if (body is null)
            return await TextResponse(req, HttpStatusCode.RequestEntityTooLarge, "Request too large");

        if (!HighScoreValidator.TryParse(body, out var dto, out var error))
            return await TextResponse(req, HttpStatusCode.BadRequest, error);

        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        if (string.IsNullOrEmpty(connectionString))
            return await TextResponse(req, HttpStatusCode.InternalServerError, "Storage not configured");

        try
        {
            var serviceClient = new TableServiceClient(connectionString);
            await serviceClient.CreateTableIfNotExistsAsync("HighScores");

            var tableClient = new TableClient(connectionString, "HighScores");
            var entity = new TableEntity("HighScore", Guid.NewGuid().ToString())
            {
                ["Name"] = dto.Name,
                ["Score"] = dto.Score
            };
            await tableClient.AddEntityAsync(entity);

            return await TextResponse(req, HttpStatusCode.OK, "Saved high score");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save high score");
            return await TextResponse(req, HttpStatusCode.InternalServerError, "Could not save high score");
        }
    }

    private static async Task<HttpResponseData> TextResponse(HttpRequestData req, HttpStatusCode status, string message)
    {
        var response = req.CreateResponse(status);
        await response.WriteStringAsync(message);
        return response;
    }
}
