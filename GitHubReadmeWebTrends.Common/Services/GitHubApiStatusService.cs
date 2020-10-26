using System;
using System.Net;
using System.Threading.Tasks;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    public class GitHubApiStatusService : GitHubApiStatus.GitHubApiStatusService
    {
        readonly GitHubRestApiService _gitHubRestApiService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GitHubApiStatusService(GitHubRestApiService gitHubRestApiService, GitHubGraphQLApiService gitHubGraphQLApiService) =>
            (_gitHubRestApiService, _gitHubGraphQLApiService) = (gitHubRestApiService, gitHubGraphQLApiService);

        public bool HasReachedMaximimApiCallLimit(in Exception exception) => exception switch
        {
            ApiException apiException when apiException.StatusCode is HttpStatusCode.Forbidden => HasReachedMaximimApiCallLimit(apiException.Headers),
            GraphQLException graphQLException => HasReachedMaximimApiCallLimit(graphQLException.ResponseHeaders),
            _ => false
        };

        public async Task<(int gitHubRestApiRequestsRemaining, int gitHubGraphQLApiRequestsRemaining)> GetRemaininRequestCount()
        {
            int gitHubRestApiRequestsRemaining, gitHubGraphQLApiRequestsRemaining;
            try
            {
                var restApiResponse = await _gitHubRestApiService.GetResponseMessage().ConfigureAwait(false);
                var graphQLApiResponse = await _gitHubGraphQLApiService.GetViewerInformationResponse().ConfigureAwait(false);

                gitHubRestApiRequestsRemaining = GetRemainingRequestCount(restApiResponse.Headers);
                gitHubGraphQLApiRequestsRemaining = GetRemainingRequestCount(graphQLApiResponse.Headers);
            }
            catch (Exception e) when (HasReachedMaximimApiCallLimit(e))
            {
                gitHubRestApiRequestsRemaining = gitHubGraphQLApiRequestsRemaining = 0;
            }

            return (gitHubRestApiRequestsRemaining, gitHubGraphQLApiRequestsRemaining);
        }
    }
}
