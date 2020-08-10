﻿using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class CreateBranchResponseModel
    {
        public CreateBranchResponseModel(CreateBranchResult createRef) => Result = createRef;

        [JsonProperty("createRef")]
        public CreateBranchResult Result { get; }
    }

    public class CreateBranchResult
    {
        public CreateBranchResult(string clientMutationId) => ClientMutationId = clientMutationId;

        [JsonProperty("clientMutationId")]
        public string ClientMutationId { get; }
    }
}