using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class CreateBranchResponse
    {
        public CreateBranchResponse(string clientMutationId) => ClientMutationId = clientMutationId;

        [JsonProperty("clientMutationId")]
        public string ClientMutationId { get; }
    }
}
