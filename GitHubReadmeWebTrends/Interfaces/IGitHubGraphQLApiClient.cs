using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;

namespace VerifyGitHubReadmeLinks
{
    [Headers("User-Agent: " + nameof(VerifyGitHubReadmeLinks))]
    interface IGitHubGraphQLApiClient
    {
        [Post("")]
        Task<GraphQLResponse<RepositoryConnectionResponse>> RepositoryConnectionQuery([Body] RepositoryConnectionQueryContent request);

        [Post("")]
        Task<GraphQLResponse<CreateBranchResponse>> CreateBranch([Body] CreateBranchMutationContent request);
    }

    public class CreateBranchMutationContent : GraphQLRequest
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
                RepositoryId = repositoryId;
                BranchName = name.Replace("/", "\\/");
                ClientMutationId = clientMutationId;

                var temp = JsonConvert.SerializeObject(BranchName);
                var temp2 = JsonConvert.SerializeObject(name);

                var temp3 = UrlEncoder.Default.EncodeUtf8(BranchName.tosp).ToString();
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

    class RepositoryConnectionQueryContent : GraphQLRequest
    {
        public RepositoryConnectionQueryContent(in string repositoryOwner, in string endCursorString, in int numberOfRepositoriesPerRequest = 100)
            : base("query { user(login:\"" + repositoryOwner + "\")  { login, repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { id, name, isFork, defaultBranchRef { id, name, prefix, target { oid } } }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }
}
