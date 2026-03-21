using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace BlazorInvaders.Api;

public class GetHighScore
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
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(ex.Message);
            return response;
        }
    }
}
