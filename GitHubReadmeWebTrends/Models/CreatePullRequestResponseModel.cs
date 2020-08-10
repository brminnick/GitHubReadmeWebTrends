using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class CreatePullRequestResponseModel
    {
        public CreatePullRequestResponseModel(MutationResultModel createPullRequest) => Result = createPullRequest;

        [JsonProperty("createPullRequest")]
        public MutationResultModel Result { get; }
    }
}
