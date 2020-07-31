using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    public class GraphQLResponse<T>
    {
        public GraphQLResponse(T data, GraphQLError[] errors) => (Data, Errors) = (data, errors);

        [JsonProperty("data")]
        public T Data { get; }

        [JsonProperty("errors")]
        public GraphQLError[] Errors { get; }
    }
}
