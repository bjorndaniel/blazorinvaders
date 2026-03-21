using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace BlazorInvaders.Api;

public class GetHighScore
{
    [Function("gethighscore")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        if (string.IsNullOrEmpty(connectionString))
            return new StatusCodeResult(500);

        try
        {
            var client = new TableClient(connectionString, "HighScores");
            var entity = await client.GetEntityAsync<TableEntity>("HighScore", "Current");
            return new OkObjectResult(new
            {
                name = entity.Value["Name"]?.ToString(),
                score = entity.Value["Score"]
            });
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new OkObjectResult(null);
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = 500 };
        }
    }
}
