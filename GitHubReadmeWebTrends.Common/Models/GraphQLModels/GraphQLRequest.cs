using System.Collections.Generic;

//Inspired by https://github.com/graphql-dotnet/graphql-dotnet/blob/11b319c0086f9276df55b2695513757923805b88/src/GraphQL.Harness/GraphQLRequest.cs
namespace GitHubReadmeWebTrends.Common
{
    public abstract record GraphQLRequest(string Query, Dictionary<string, object>? Variables = null);
}
