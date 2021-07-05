namespace GitHubReadmeWebTrends.Common
{
    public record CreateForkResponseModel(string Node_id, string Name, string Default_branch, Owner Owner);

    public record Owner(string Login);
}