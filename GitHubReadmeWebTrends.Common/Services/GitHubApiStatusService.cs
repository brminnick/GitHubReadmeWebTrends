using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GitHubApiStatus;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    public class GitHubApiStatusService : GitHubApiStatus.GitHubApiStatusService
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;

        public bool HasReachedMaximimApiCallLimit(in Exception exception) => exception switch
        {
            ApiException apiException when apiException.StatusCode is HttpStatusCode.Forbidden => HasReachedMaximimApiCallLimit(apiException.Headers),
            GraphQLException graphQLException => HasReachedMaximimApiCallLimit(graphQLException.ResponseHeaders),
            _ => false
        };

        public Task<GitHubApiRateLimits> GetApiRateLimits() => GetApiRateLimits(new AuthenticationHeaderValue("bearer", _token));
    }
}
