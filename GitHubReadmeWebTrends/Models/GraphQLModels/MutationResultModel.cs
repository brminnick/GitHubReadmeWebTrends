using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class MutationResultModel
    {
        public MutationResultModel(string clientMutationId) => ClientMutationId = clientMutationId;

        [JsonProperty("clientMutationId")]
        public string ClientMutationId { get; }
    }
}
