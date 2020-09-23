using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class CreatePullRequestResponseModel
    {
        public CreatePullRequestResponseModel(MutationResultModel createPullRequest) => Result = createPullRequest;

        [JsonProperty("createPullRequest")]
        public MutationResultModel Result { get; }
    }
}
