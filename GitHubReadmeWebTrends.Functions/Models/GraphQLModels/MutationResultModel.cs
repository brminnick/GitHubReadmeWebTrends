using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks.Functions
{
    class MutationResultModel
    {
        public MutationResultModel(string clientMutationId) => ClientMutationId = clientMutationId;

        [JsonProperty("clientMutationId")]
        public string ClientMutationId { get; }
    }
}
