using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitHubReadmeWebTrends.Common
{
    public record GraphQLError(string Message, GraphQLLocation[] Locations)
    {
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? AdditonalEntries { get; set; }
    }

    public record GraphQLLocation(long Line, long Column);
}
