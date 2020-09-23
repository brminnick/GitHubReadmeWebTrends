using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class CreateBranchResponseModel
    {
        public CreateBranchResponseModel(MutationResultModel createRef) => Result = createRef;

        [JsonProperty("createRef")]
        public MutationResultModel Result { get; }
    }
}
