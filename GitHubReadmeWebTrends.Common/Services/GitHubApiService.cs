using System;
using System.Net;
using GitHubApiStatus;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    public class GitHubApiExceptionService : GitHubApiStatusService
    {
        public bool HasReachedMaximimApiCallLimit(in Exception exception) => exception switch
        {
            ApiException apiException when apiException.StatusCode is HttpStatusCode.Forbidden => HasReachedMaximimApiCallLimit(apiException.Headers),
            GraphQLException graphQLException => HasReachedMaximimApiCallLimit(graphQLException.ResponseHeaders),
            _ => false
        };
    }
}
