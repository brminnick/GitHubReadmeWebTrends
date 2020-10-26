using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    public class GitHubGraphQLApiService
    {
        readonly IGitHubGraphQLApiClient _gitHubGraphQLApiClient;

        public GitHubGraphQLApiService(IGitHubGraphQLApiClient gitHubGraphQLApiClient) => _gitHubGraphQLApiClient = gitHubGraphQLApiClient;


        public Task<CreateBranchResponseModel> CreateBranch(string repositoryId, string repositoryName, string branchOid, Guid guid) =>
            GetGraphQLResponseData(CreateBranchResponse(repositoryId, repositoryName, branchOid, guid));

        public Task<RepositoryConnectionResponse> GetRepository(string repositoryOwner, string repositoryName) =>
           GetGraphQLResponseData(GetRepositoryResponse(repositoryOwner, repositoryName));

        public Task<CreatePullRequestResponseModel> CreatePullRequest(in string repositoryId, in string baseRefName, in string headRefName, in string title, in string body, in Guid clientMutationId, in bool maintainerCanModify = true, in bool draft = false) =>
            GetGraphQLResponseData(CreatePullRequestResponse(repositoryId, baseRefName, headRefName, title, body, clientMutationId, maintainerCanModify, draft));

        public Task<GitHubViewerResponse> GetViewerInformation() =>
            GetGraphQLResponseData(GetViewerInformationResponse());

        public Task<ApiResponse<GraphQLResponse<CreateBranchResponseModel>>> CreateBranchResponse(string repositoryId, string repositoryName, string branchOid, Guid guid) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.CreateBranchMutation(new CreateBranchMutationContent(repositoryId, repositoryName, branchOid, guid)));       

        public Task<ApiResponse<GraphQLResponse<RepositoryConnectionResponse>>> GetRepositoryResponse(string repositoryOwner, string repositoryName) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.RepositoryConnectionQuery(new RepositoryConnectionQueryContent(repositoryOwner, repositoryName)));        

        public Task<ApiResponse<GraphQLResponse<CreatePullRequestResponseModel>>> CreatePullRequestResponse(in string repositoryId, in string baseRefName, in string headRefName, in string title, in string body, in Guid clientMutationId, in bool maintainerCanModify = true, in bool draft = false) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.CreatePullRequestMutation(new CreatePullRequestMutationContent(repositoryId, baseRefName, headRefName, title, body, clientMutationId, maintainerCanModify, draft)));

        public Task<ApiResponse<GraphQLResponse<GitHubViewerResponse>>> GetViewerInformationResponse() =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.ViewerLoginQuery(new ViewerLoginQueryContent()));

        public async IAsyncEnumerable<IEnumerable<Repository>> GetRepositories(string repositoryOwner, int numberOfRepositoriesPerRequest = 100)
        {
            RepositoriesConnectionResponse? repositoryConnection = null;

            do
            {
                repositoryConnection = await GetRepositoryConnectionResponse(repositoryOwner, repositoryConnection?.PageInfo?.EndCursor, numberOfRepositoriesPerRequest).ConfigureAwait(false);

                yield return repositoryConnection?.RepositoryList ?? Enumerable.Empty<Repository>();
            }
            while (repositoryConnection?.PageInfo?.HasNextPage is true);
        }

        async Task<T> GetGraphQLResponseData<T>(Task<ApiResponse<GraphQLResponse<T>>> graphQLRequestTask)
        {
            var response = await ExecuteGraphQLRequest(graphQLRequestTask).ConfigureAwait(false);
            return response.Content.Data;
        }

        async Task<ApiResponse<GraphQLResponse<T>>> ExecuteGraphQLRequest<T>(Task<ApiResponse<GraphQLResponse<T>>> graphQLRequestTask)
        {
            var response = await graphQLRequestTask.ConfigureAwait(false);

            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);

            if (response.Content.Errors != null)
                throw new GraphQLException(response.Content.Errors, response.Headers, response.StatusCode);

            return response;
        }

        async Task<RepositoriesConnectionResponse> GetRepositoryConnectionResponse(string repositoryOwner, string? endCursor, int numberOfRepositoriesPerRequest = 100)
        {
            var apiResponse = await ExecuteGraphQLRequest(_gitHubGraphQLApiClient.RepositoriesConnectionQuery(new RepositoriesConnectionQueryContent(repositoryOwner, getEndCursorString(endCursor), numberOfRepositoriesPerRequest))).ConfigureAwait(false);
            return apiResponse.Content.Data;

            static string getEndCursorString(string? endCursor) => string.IsNullOrWhiteSpace(endCursor) ? string.Empty : "after: \"" + endCursor + "\"";
        }
    }
}
