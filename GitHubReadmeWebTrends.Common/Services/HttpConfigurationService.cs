using System;
using System.Net;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace GitHubReadmeWebTrends.Common
{
    public static class HttpConfigurationService
    {
        public static DecompressionMethods GetDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;
        public static IAsyncPolicy<HttpResponseMessage> GetPolicyHandler() => HttpPolicyExtensions.HandleTransientHttpError().OrResult(msg => msg.StatusCode is HttpStatusCode.Forbidden).WaitAndRetryAsync(10, count => TimeSpan.FromSeconds(60));
    }
}