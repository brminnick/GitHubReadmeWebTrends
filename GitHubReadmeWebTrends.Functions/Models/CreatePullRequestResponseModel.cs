using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks.Functions
{
    class CreatePullRequestResponseModel
    {
        public CreatePullRequestResponseModel(MutationResultModel createPullRequest) => Result = createPullRequest;

        [JsonProperty("createPullRequest")]
        public MutationResultModel Result { get; }
    }
}
