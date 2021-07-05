namespace GitHubReadmeWebTrends.Common
{
    public record GraphQLResponse<T>(T Data, GraphQLError[] Errors);
}
