using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace BlazorInvaders.Api;

internal static class FunctionResponses
{
    public const string StorageSetting = "AzureWebJobsStorage";
    public const string TableName = "HighScores";

    public static async Task<HttpResponseData> TextAsync(HttpRequestData req, HttpStatusCode status, string message)
    {
        var response = req.CreateResponse(status);
        await response.WriteStringAsync(message);
        return response;
    }
}
