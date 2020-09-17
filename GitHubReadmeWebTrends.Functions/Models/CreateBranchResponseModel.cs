using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks.Functions
{
    class CreateBranchResponseModel
    {
        public CreateBranchResponseModel(MutationResultModel createRef) => Result = createRef;

        [JsonProperty("createRef")]
        public MutationResultModel Result { get; }
    }
}
