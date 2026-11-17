using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace BlazorInvaders.Api;

public class SaveHighScore(ILogger<SaveHighScore> logger, HighScoreStore store)
{
    [Function("savehighscore")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var body = await HighScoreValidator.ReadBoundedAsync(req.Body, HighScoreValidator.MaxBodyBytes);
        if (body is null)
            return await FunctionResponses.TextAsync(req, HttpStatusCode.RequestEntityTooLarge, "Request too large");

        if (!HighScoreValidator.TryParse(body, out var dto, out var error))
            return await FunctionResponses.TextAsync(req, HttpStatusCode.BadRequest, error);

        if (!store.IsConfigured)
            return await FunctionResponses.TextAsync(req, HttpStatusCode.InternalServerError, "Storage not configured");

        try
        {
            // All-time scores and each day's challenge live in separate partitions
            await store.AddAsync(dto);

            return await FunctionResponses.TextAsync(req, HttpStatusCode.OK, "Saved high score");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save high score");
            return await FunctionResponses.TextAsync(req, HttpStatusCode.InternalServerError, "Could not save high score");
        }
    }
}
