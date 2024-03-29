﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    public class GitHubGraphQLApiService
    {
        readonly static IReadOnlyList<(string Owner, string Repository)> _microsoftLearnRepositories = new[]
        {
           ("microsoftdocs", "learnshared"),
           ("microsoftdocs", "learn-certs-pr"),
           ("microsoftdocs", "learn-m365-pr"),
           ("microsoftdocs", "learn-bizapps-pr"),
           ("microsoftdocs", "learn-dynamics-pr"),
           ("microsoftdocs", "learn-pr"),
        };

        readonly IGitHubGraphQLApiClient _gitHubGraphQLApiClient;

        public GitHubGraphQLApiService(IGitHubGraphQLApiClient gitHubGraphQLApiClient) => _gitHubGraphQLApiClient = gitHubGraphQLApiClient;

        public async Task<ContributionsCollectionModel> GetMicrosoftDocsContributionsCollection(string gitHubUserName, DateTimeOffset from, DateTimeOffset to)
        {
            var response = await GetContributionsCollection(gitHubUserName, "MDEyOk9yZ2FuaXphdGlvbjIyNDc5NDQ5", from, to).ConfigureAwait(false);

            if (response?.Content is null)
                throw new JsonException();

            return response.Content.Data.User.ContributionsCollection;
        }

        public async IAsyncEnumerable<IReadOnlyList<RepositoryPullRequest>> GetMicrosoftLearnPullRequests()
        {
            foreach (var repo in _microsoftLearnRepositories)
            {
                var repositoryPullRequests = new List<RepositoryPullRequest>();
                    
                await Parallel.ForEachAsync(GetDefaultBranchPullRequests(repo.Repository, repo.Owner), (result, token) =>
                {
                    var pullRequestResults = result.Select(x => new RepositoryPullRequest(repo.Repository, repo.Owner, x.Id, x.Url, x.CreatedAt, x.Merged, x.MergedAt, x.BaseRefName, x.Author));
                    repositoryPullRequests.AddRange(pullRequestResults);

                    return ValueTask.CompletedTask;
                }).ConfigureAwait(false);

                yield return repositoryPullRequests;
            }
        }

        public async Task<Repository?> GetRepository(string repositoryOwner, string repositoryName)
        {
            var response = await GetGraphQLResponseData(GetRepositoryResponse(repositoryOwner, repositoryName)).ConfigureAwait(false);
            return response.User.Repository switch
            {
                null => null,
                _ => new Repository(response.User.Repository.Id, response.User.Login, response.User.Repository.Name, response.User.Repository.DefaultBranchRef, response.User.Repository.IsFork)
            };
        }

        public Task<CreatePullRequestResponseModel> CreatePullRequest(in string repositoryId, in string baseRefName, in string headRefName, in string title, in string body, in Guid clientMutationId, in bool maintainerCanModify = true, in bool draft = false) =>
            GetGraphQLResponseData(CreatePullRequestResponse(repositoryId, baseRefName, headRefName, title, body, clientMutationId, maintainerCanModify, draft));

        public Task<GitHubViewerResponse> GetViewerInformation() =>
            GetGraphQLResponseData(GetViewerInformationResponse());

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

        public Task<CreateBranchResponseModel> CreateBranch(string repositoryId, string repositoryRefBranchName, string branchOid, Guid guid) =>
            GetGraphQLResponseData(CreateBranchResponse(repositoryId, repositoryRefBranchName, branchOid, guid));

        static string GetEndCursorString(string? endCursor) => string.IsNullOrWhiteSpace(endCursor) ? string.Empty : "after: \"" + endCursor + "\"";

        static async Task<ApiResponse<GraphQLResponse<T>>> ExecuteGraphQLRequest<T>(Task<ApiResponse<GraphQLResponse<T>>> graphQLRequestTask)
        {
            var response = await graphQLRequestTask.ConfigureAwait(false);
            if (response?.Content is null)
                throw new JsonException();

            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);

            if (response.Content.Errors != null)
                throw new GraphQLException(response.Content.Errors, response.Headers, response.StatusCode);

            return response;
        }

        Task<ApiResponse<GraphQLResponse<ContributionsResponse>>> GetContributionsCollection(string gitHubUserName, string organizationId, DateTimeOffset from, DateTimeOffset to) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.ContributionsQuery(new ContributionsQueryContent(gitHubUserName, organizationId, from, to)));

        Task<ApiResponse<GraphQLResponse<CreateBranchResponseModel>>> CreateBranchResponse(string repositoryId, string repositoryName, string branchOid, Guid guid) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.CreateBranchMutation(new CreateBranchMutationContent(repositoryId, repositoryName, branchOid, guid)));

        Task<ApiResponse<GraphQLResponse<RepositoryConnectionResponse>>> GetRepositoryResponse(string repositoryOwner, string repositoryName) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.RepositoryConnectionQuery(new RepositoryConnectionQueryContent(repositoryOwner, repositoryName)));

        Task<ApiResponse<GraphQLResponse<CreatePullRequestResponseModel>>> CreatePullRequestResponse(in string repositoryId, in string baseRefName, in string headRefName, in string title, in string body, in Guid clientMutationId, in bool maintainerCanModify = true, in bool draft = false) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.CreatePullRequestMutation(new CreatePullRequestMutationContent(repositoryId, baseRefName, headRefName, title, body, clientMutationId, maintainerCanModify, draft)));

        Task<ApiResponse<GraphQLResponse<GitHubViewerResponse>>> GetViewerInformationResponse() =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.ViewerLoginQuery(new ViewerLoginQueryContent()));

        async IAsyncEnumerable<IEnumerable<PullRequest>> GetDefaultBranchPullRequests(string repositoryName, string repositoryOwner)
        {
            RepositoryPullRequestResponse? repositoryPullRequestResponse = null;
            do
            {
                repositoryPullRequestResponse = await GetRepositoryPullRequestResponse(repositoryName, repositoryOwner, repositoryPullRequestResponse?.Repository.PullRequests?.PageInfo?.EndCursor).ConfigureAwait(false);
                yield return repositoryPullRequestResponse?.Repository.PullRequests?.Nodes.Where(x => x.BaseRefName == repositoryPullRequestResponse.Repository.DefaultBranchRef.Name) ?? Enumerable.Empty<PullRequest>();
            }
            while (repositoryPullRequestResponse?.Repository.PullRequests?.PageInfo?.HasNextPage is true);
        }

        async Task<RepositoryPullRequestResponse?> GetRepositoryPullRequestResponse(string repositoryName, string repositoryOwner, string? endCursor, int numberOfPullRequestsPerRequest = 100)
        {
            var response = await ExecuteGraphQLRequest(_gitHubGraphQLApiClient.RepositoryPullRequestQuery(new RepositoryPullRequestQueryContent(repositoryName, repositoryOwner, GetEndCursorString(endCursor), numberOfPullRequestsPerRequest))).ConfigureAwait(false);

            if (response?.Content is null)
                throw new JsonException();

            return response.Content.Data;
        }

        async Task<RepositoriesConnectionResponse> GetRepositoryConnectionResponse(string repositoryOwner, string? endCursor, int numberOfRepositoriesPerRequest = 100)
        {
            var response = await ExecuteGraphQLRequest(_gitHubGraphQLApiClient.RepositoriesConnectionQuery(new RepositoriesConnectionQueryContent(repositoryOwner, GetEndCursorString(endCursor), numberOfRepositoriesPerRequest))).ConfigureAwait(false);

            if (response?.Content is null)
                throw new JsonException();

            return response.Content.Data;
        }

        async Task<T> GetGraphQLResponseData<T>(Task<ApiResponse<GraphQLResponse<T>>> graphQLRequestTask)
        {
            var response = await ExecuteGraphQLRequest(graphQLRequestTask).ConfigureAwait(false);

            if (response?.Content is null)
                throw new JsonException();

            return response.Content.Data;
        }
    }
}
