using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    [Headers("User-Agent: " + nameof(GitHubReadmeWebTrends))]
    public interface IGitHubGraphQLApiClient
    {
        [Post("")]
        Task<ApiResponse<GraphQLResponse<RepositoriesConnectionResponse>>> RepositoriesConnectionQuery([Body] RepositoriesConnectionQueryContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<RepositoryConnectionResponse>>> RepositoryConnectionQuery([Body] RepositoryConnectionQueryContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<CreateBranchResponseModel>>> CreateBranchMutation([Body] CreateBranchMutationContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<CreatePullRequestResponseModel>>> CreatePullRequestMutation([Body] CreatePullRequestMutationContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<GitHubViewerResponse>>> ViewerLoginQuery([Body] ViewerLoginQueryContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<ContributionsResponse>>> ContributionsQuery([Body] ContributionsQueryContent request);

        [Post("")]
        Task<ApiResponse<GraphQLResponse<RepositoryPullRequestResponse>>> RepositoryPullRequestQuery([Body] RepositoryPullRequestQueryContent request);
    }

    public class ContributionsQueryContent : GraphQLRequest
    {
        public ContributionsQueryContent(string gitHubLogin, string organizationId, DateTimeOffset from, DateTimeOffset to)
            : base("query { user(login: \"" + gitHubLogin + "\") { contributionsCollection(organizationID: \"" + organizationId + "\", from: " + JsonConvert.SerializeObject(from) + ", to: " + JsonConvert.SerializeObject(to) + ") { totalIssueContributions, totalCommitContributions, totalRepositoryContributions, totalPullRequestContributions, totalPullRequestReviewContributions commitContributionsByRepository(maxRepositories: 100) { repository { name }, }, issueContributionsByRepository(maxRepositories: 100) { repository { name }, }, pullRequestContributionsByRepository(maxRepositories:100) { repository { name } }, pullRequestReviewContributionsByRepository(maxRepositories: 100) { repository { name }}}}}")
        {

        }
    }

    public class CreatePullRequestMutationContent : GraphQLRequest
    {
        const string createPullRequest = "createPullRequest";

        public CreatePullRequestMutationContent(in string repositoryId, in string baseRefName, in string headRefName,
                                                    in string title, in string body, in Guid clientMutationId,
                                                    in bool maintainerCanModify = true, in bool draft = false)

            : base("mutation($" + createPullRequest + ":CreatePullRequestInput!){" + createPullRequest + "(input:$" + createPullRequest + ") { clientMutationId } }",
                    new Dictionary<string, object> { { createPullRequest, new CreatePullRequestModel(repositoryId, baseRefName, headRefName, title, body, clientMutationId, maintainerCanModify, draft) } })
        {
        }

        class CreatePullRequestModel
        {
            public CreatePullRequestModel(in string repositoryId, in string baseRefName, in string headRefName, in string title,
                                            in string body, in Guid clientMutationId, in bool maintainerCanModify, in bool draft)
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

            [JsonProperty("repositoryId")]
            public string RepositoryId { get; }

            [JsonProperty("baseRefName")]
            public string BaseRefName { get; }

            [JsonProperty("headRefName")]
            public string HeadRefName { get; }

            [JsonProperty("title")]
            public string Title { get; }

            [JsonProperty("body")]
            public string Body { get; }

            [JsonProperty("clientMutationId")]
            public Guid ClientMutationId { get; }

            [JsonProperty("maintainerCanModify")]
            public bool MaintainerCanModify { get; }

            [JsonProperty("draft")]
            public bool Draft { get; }
        }
    }

    public class CreateBranchMutationContent : GraphQLRequest
    {
        const string createRef = "createRef";

        public CreateBranchMutationContent(in string repositoryId, in string branchName, in string branchOid, in Guid guid)
            : base("mutation($" + createRef + ":CreateRefInput!){" + createRef + "(input:$" + createRef + ") { clientMutationId } }",
                    new Dictionary<string, object> { { createRef, new CreateBranchRequestModel(repositoryId, branchName, branchOid, guid.ToString()) } })
        {

        }

        class CreateBranchRequestModel
        {
            public CreateBranchRequestModel(in string repositoryId, in string name, in string oid, in string clientMutationId)
            {
                Oid = oid;
                BranchName = name;
                RepositoryId = repositoryId;
                ClientMutationId = clientMutationId;
            }

            [JsonProperty("repositoryId")]
            public string RepositoryId { get; }

            [JsonProperty("name")]
            public string BranchName { get; }

            [JsonProperty("oid")]
            public string Oid { get; }

            [JsonProperty("clientMutationId")]
            public string ClientMutationId { get; }
        }
    }

    public class RepositoryPullRequestQueryContent : GraphQLRequest
    {
        public RepositoryPullRequestQueryContent(in string repositoryName, in string repositoryOwner, in string endCursorString, in int numberOfPullRewuestsPerRequest = 100)
            : base("query { repository(name: \"" + repositoryName + "\", owner: \"" + repositoryOwner + "\")  { defaultBranchRef { name } pullRequests(first: " + numberOfPullRewuestsPerRequest + endCursorString + ") { nodes { url, id, createdAt, merged, mergedAt, baseRefName, author { login } } pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }

    public class RepositoriesConnectionQueryContent : GraphQLRequest
    {
        public RepositoriesConnectionQueryContent(in string repositoryOwner, in string endCursorString, in int numberOfRepositoriesPerRequest = 100)
            : base("query { user(login:\"" + repositoryOwner + "\")  { login, repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { id, name, isFork, owner { login }, defaultBranchRef { id, name, prefix, target { oid } } }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }

    public class RepositoryConnectionQueryContent : GraphQLRequest
    {
        public RepositoryConnectionQueryContent(in string repositoryOwner, in string repositoryName)
            : base("query { user(login:\"" + repositoryOwner + "\") { login, repository(name:\"" + repositoryName + "\"){ id, name, isFork, defaultBranchRef { id, name, prefix, target { oid } } } } }")
        {

        }
    }

    public class ViewerLoginQueryContent : GraphQLRequest
    {
        public ViewerLoginQueryContent() : base("query { viewer { name, login, email } }")
        {

        }
    }
}
