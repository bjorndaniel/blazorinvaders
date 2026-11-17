using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace BlazorInvaders.Api;

public class GetLeaderboard(ILogger<GetLeaderboard> logger, HighScoreStore store)
{
    [Function("getleaderboard")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        // ?mode=daily[&day=yyyyMMdd] for a daily challenge board; all-time otherwise
        if (!HighScoreValidator.TryGetLeaderboardPartition(req.Query["mode"], req.Query["day"], out var partition, out var queryError))
        {
            return await FunctionResponses.TextAsync(req, HttpStatusCode.BadRequest, queryError);
        }

        if (!store.IsConfigured)
            return await FunctionResponses.TextAsync(req, HttpStatusCode.InternalServerError, "Storage not configured");

        try
        {
            var top10 = (await store.GetTopAsync(partition, req.FunctionContext.CancellationToken))
                .Select(e => new { name = e.Name, score = e.Score });

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(top10));
            return response;
        }
        catch (OperationCanceledException) when (req.FunctionContext.CancellationToken.IsCancellationRequested)
        {
            return await FunctionResponses.TextAsync(req, HttpStatusCode.InternalServerError, "Could not load leaderboard");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load leaderboard");
            return await FunctionResponses.TextAsync(req, HttpStatusCode.InternalServerError, "Could not load leaderboard");
        }
    }
}
