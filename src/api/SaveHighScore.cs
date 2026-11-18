using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;

namespace BlazorInvaders.Api;

public class SaveHighScore(ILogger<SaveHighScore> logger, HighScoreStore store, ScoreRateLimiter limiter, TimeProvider time)
{
    [Function("savehighscore")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        // Checked first, before the body is read, so every post counts
        var address = ScoreRateLimiter.ClientAddress(Header(req, "X-Client-IP"), Header(req, "X-Forwarded-For"));
        if (!limiter.TryAcquire(address, time.GetUtcNow(), out var retryAfter, out var bucket))
        {
            // The bucket kind shows a missing client address (unknown); the address and key are never logged
            logger.LogWarning("Score post rejected: over the post limit, {Bucket} bucket", bucket);
            var limited = req.CreateResponse(HttpStatusCode.TooManyRequests);
            limited.Headers.Add("Retry-After", ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture));
            await limited.WriteStringAsync("Too many scores, try again later");
            return limited;
        }

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

    private static string? Header(HttpRequestData req, string name) =>
        req.Headers.TryGetValues(name, out var values) ? string.Join(",", values) : null;
}
