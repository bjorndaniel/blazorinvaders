using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace BlazorInvaders.Api;

public class GetHighScore(ILogger<GetHighScore> logger)
{
    [Function("gethighscore")]
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
            var client = new TableClient(connectionString, "HighScores");
            var entity = await client.GetEntityAsync<TableEntity>("HighScore", "Current");
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                name = entity.Value["Name"]?.ToString(),
                score = entity.Value["Score"]
            });
            return response;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("null");
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load high score");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Could not load high score");
            return response;
        }
    }
}
