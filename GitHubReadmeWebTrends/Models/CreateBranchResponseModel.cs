using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class CreateBranchResponseModel
    {
        public CreateBranchResponseModel(MutationResultModel createRef) => Result = createRef;

        [JsonProperty("createRef")]
        public MutationResultModel Result { get; }
    }
}
