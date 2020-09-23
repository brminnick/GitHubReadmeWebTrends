using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class MutationResultModel
    {
        public MutationResultModel(string clientMutationId) => ClientMutationId = clientMutationId;

        [JsonProperty("clientMutationId")]
        public string ClientMutationId { get; }
    }
}
