using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace BlazorInvaders.Api;

public record HighScoreRequest(string Name, int Score);

public class SaveHighScore
{
    [Function("savehighscore")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        HighScoreRequest? dto;
        try
        {
            dto = await req.ReadFromJsonAsync<HighScoreRequest>();
        }
        catch
        {
            dto = null;
        }

        if (dto is null || string.IsNullOrEmpty(dto.Name))
            return new BadRequestObjectResult("Missing name or score");

        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        if (string.IsNullOrEmpty(connectionString))
            return new StatusCodeResult(500);

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

            return new OkObjectResult("Saved high score");
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = 500 };
        }
    }
}
