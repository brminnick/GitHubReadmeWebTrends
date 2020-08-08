using System.Collections.Generic;
using Newtonsoft.Json;

//Inspired by https://github.com/graphql-dotnet/graphql-dotnet/blob/11b319c0086f9276df55b2695513757923805b88/src/GraphQL.Harness/GraphQLRequest.cs
namespace VerifyGitHubReadmeLinks
{
    public abstract class GraphQLRequest
    {
        protected GraphQLRequest(string query, Dictionary<string, object>? variables = null) =>
            (Query, Variables) = (query, variables ?? new Dictionary<string, object>());

        [JsonProperty("query")]
        public string Query { get; }

        [JsonProperty("variables")]
        public Dictionary<string, object> Variables { get; }
    }
}
