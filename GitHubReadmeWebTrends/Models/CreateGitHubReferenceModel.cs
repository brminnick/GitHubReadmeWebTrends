using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class CreateGitHubReferenceModel
    {
        public CreateGitHubReferenceModel(string reference, string sha) =>
        (Reference, Sha) = (reference, sha);

        [JsonProperty("ref")]
        public string Reference { get; }

        [JsonProperty("sha")]
        public string Sha { get; }
    }
}
