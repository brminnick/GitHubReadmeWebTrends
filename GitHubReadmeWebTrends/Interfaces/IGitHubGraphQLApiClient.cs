using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;

namespace VerifyGitHubReadmeLinks
{
    [Headers("User-Agent: " + nameof(VerifyGitHubReadmeLinks))]
    interface IGitHubGraphQLApiClient
    {
        [Post("")]
        Task<GraphQLResponse<RepositoriesConnectionResponse>> RepositoriesConnectionQuery([Body] RepositoriesConnectionQueryContent request);

        [Post("")]
        Task<GraphQLResponse<RepositoryConnectionResponse>> RepositoryConnectionQuery([Body] RepositoryConnectionQueryContent request);

        [Post("")]
        Task<GraphQLResponse<CreateBranchResponseModel>> CreateBranchQuery([Body] CreateBranchMutationContent request);

        [Post("")]
        Task<GraphQLResponse<GitHubViewerResponse>> ViewerLoginQuery([Body] ViewerLoginQueryContent request);
    }

    class CreateBranchMutationContent : GraphQLRequest
    {
        const string createRef = "createRef";

        public CreateBranchMutationContent(in string repositoryId, in string branchName, in string branchOid, Guid guid)
            : base("mutation($" + createRef + ":CreateRefInput!){ createRef(input:$" + createRef + ") { clientMutationId } }",
                    new Dictionary<string, object> { { createRef, new CreateBranchRequestModel(repositoryId, branchName, branchOid, guid.ToString()) } })
        {

        }

        [JsonObject(createRef)]
        class CreateBranchRequestModel
        {
            public CreateBranchRequestModel(string repositoryId, string name, string oid, string clientMutationId)
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

    class RepositoriesConnectionQueryContent : GraphQLRequest
    {
        public RepositoriesConnectionQueryContent(in string repositoryOwner, in string endCursorString, in int numberOfRepositoriesPerRequest = 100)
            : base("query { user(login:\"" + repositoryOwner + "\")  { login, repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { id, name, isFork, defaultBranchRef { id, name, prefix, target { oid } } }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }

    class RepositoryConnectionQueryContent : GraphQLRequest
    {
        public RepositoryConnectionQueryContent(in string repositoryOwner, in string repositoryName)
            : base("query { user(login:\"" + repositoryOwner + "\") { login, repository(name:\"" + repositoryName + "\"){ id, name, defaultBranchRef { id, name, prefix, target { oid } } } } }")
        {

        }
    }

    class ViewerLoginQueryContent : GraphQLRequest
    {
        public ViewerLoginQueryContent() : base("query { viewer { name, login, email } }")
        {

        }
    }
}
