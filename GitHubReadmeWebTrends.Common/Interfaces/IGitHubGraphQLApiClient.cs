using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    [Headers("User-Agent: " + nameof(GitHubReadmeWebTrends))]
    public interface IGitHubGraphQLApiClient
    {
        [Post("")]
        Task<ApiResponse<GraphQLResponse<RepositoriesConnectionResponse>>> RepositoriesConnectionQuery([Body(true)] RepositoriesConnectionQueryContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<RepositoryConnectionResponse>>> RepositoryConnectionQuery([Body(true)] RepositoryConnectionQueryContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<CreateBranchResponseModel>>> CreateBranchMutation([Body(true)] CreateBranchMutationContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<CreatePullRequestResponseModel>>> CreatePullRequestMutation([Body(true)] CreatePullRequestMutationContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<GitHubViewerResponse>>> ViewerLoginQuery([Body(true)] ViewerLoginQueryContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<ContributionsResponse>>> ContributionsQuery([Body(true)] ContributionsQueryContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<RepositoryPullRequestResponse>>> RepositoryPullRequestQuery([Body(true)] RepositoryPullRequestQueryContent request);
    }

    public record ContributionsQueryContent : GraphQLRequest
    {
        public ContributionsQueryContent(string gitHubLogin, string organizationId, DateTimeOffset from, DateTimeOffset to)
            : base("query { user(login: \"" + gitHubLogin + "\") { contributionsCollection(organizationID: \"" + organizationId + "\", from: " + JsonSerializer.Serialize(from) + ", to: " + JsonSerializer.Serialize(to) + ") { totalIssueContributions, totalCommitContributions, totalRepositoryContributions, totalPullRequestContributions, totalPullRequestReviewContributions commitContributionsByRepository(maxRepositories: 100) { repository { name }, }, issueContributionsByRepository(maxRepositories: 100) { repository { name }, }, pullRequestContributionsByRepository(maxRepositories:100) { repository { name } }, pullRequestReviewContributionsByRepository(maxRepositories: 100) { repository { name }}}}}")
        {

        }
    }

    public record CreatePullRequestMutationContent : GraphQLRequest
    {
        const string createPullRequest = "createPullRequest";

        public CreatePullRequestMutationContent(string repositoryId, string baseRefName, string headRefName,
                                                    string title, string body, Guid clientMutationId,
                                                    bool maintainerCanModify = true, bool draft = false)

            : base("mutation($" + createPullRequest + ":CreatePullRequestInput!){" + createPullRequest + "(input:$" + createPullRequest + ") { clientMutationId } }",
                    new Dictionary<string, object> { { createPullRequest, new CreatePullRequestModel(repositoryId, baseRefName, headRefName, title, body, clientMutationId, maintainerCanModify, draft) } })
        {
        }

        record CreatePullRequestModel
        {
            public CreatePullRequestModel(string repositoryId, string baseRefName, string headRefName, string title,
                                            string body, Guid clientMutationId, bool maintainerCanModify, bool draft)
            {
                RepositoryId = repositoryId;
                BaseRefName = baseRefName;
                HeadRefName = headRefName;
                Title = title;
                Body = body;
                ClientMutationId = clientMutationId;
                MaintainerCanModify = maintainerCanModify;
                Draft = draft;
            }

            [JsonPropertyName("repositoryId")]
            public string RepositoryId { get; }

            [JsonPropertyName("baseRefName")]
            public string BaseRefName { get; }

            [JsonPropertyName("headRefName")]
            public string HeadRefName { get; }

            [JsonPropertyName("title")]
            public string Title { get; }

            [JsonPropertyName("body")]
            public string Body { get; }

            [JsonPropertyName("clientMutationId")]
            public Guid ClientMutationId { get; }

            [JsonPropertyName("maintainerCanModify")]
            public bool MaintainerCanModify { get; }

            [JsonPropertyName("draft")]
            public bool Draft { get; }
        }
    }

    public record CreateBranchMutationContent : GraphQLRequest
    {
        const string createRef = "createRef";

        public CreateBranchMutationContent(string repositoryId, string branchName, string branchOid, Guid guid)
            : base("mutation($" + createRef + ":CreateRefInput!){" + createRef + "(input:$" + createRef + ") { clientMutationId } }",
                    new Dictionary<string, object> { { createRef, new CreateBranchRequestModel(repositoryId, branchName, branchOid, guid.ToString()) } })
        {

        }

        record CreateBranchRequestModel
        {
            public CreateBranchRequestModel(string repositoryId, string name, string oid, string clientMutationId)
            {
                Oid = oid;
                BranchName = name;
                RepositoryId = repositoryId;
                ClientMutationId = clientMutationId;
            }

            [JsonPropertyName("repositoryId")]
            public string RepositoryId { get; }

            [JsonPropertyName("name")]
            public string BranchName { get; }

            [JsonPropertyName("oid")]
            public string Oid { get; }

            [JsonPropertyName("clientMutationId")]
            public string ClientMutationId { get; }
        }
    }

    public record RepositoryPullRequestQueryContent : GraphQLRequest
    {
        public RepositoryPullRequestQueryContent(string repositoryName, string repositoryOwner, string endCursorString, int numberOfPullRewuestsPerRequest = 100)
            : base("query { repository(name: \"" + repositoryName + "\", owner: \"" + repositoryOwner + "\")  { defaultBranchRef { name } pullRequests(first: " + numberOfPullRewuestsPerRequest + endCursorString + ") { nodes { url, id, createdAt, merged, mergedAt, baseRefName, author { login } } pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }

    public record RepositoriesConnectionQueryContent : GraphQLRequest
    {
        public RepositoriesConnectionQueryContent(string repositoryOwner, string endCursorString, int numberOfRepositoriesPerRequest = 100)
            : base("query { user(login:\"" + repositoryOwner + "\")  { login, repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { id, name, isFork, owner { login }, defaultBranchRef { id, name, prefix, target { oid } } }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }

    public record RepositoryConnectionQueryContent : GraphQLRequest
    {
        public RepositoryConnectionQueryContent(string repositoryOwner, string repositoryName)
            : base("query { user(login:\"" + repositoryOwner + "\") { login, repository(name:\"" + repositoryName + "\"){ id, name, isFork, defaultBranchRef { id, name, prefix, target { oid } } } } }")
        {

        }
    }

    public record ViewerLoginQueryContent : GraphQLRequest
    {
        public ViewerLoginQueryContent() : base("query { viewer { name, login, email } }")
        {

        }
    }
}
