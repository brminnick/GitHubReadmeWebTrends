namespace GitHubReadmeWebTrends.Common
{
    public record RepositoryConnectionResponse(User_RepositoryConnectionResponse User);

    public record User_RepositoryConnectionResponse(string Login, Repository_RepositoryConnectionResponse? Repository);

    public record Repository_RepositoryConnectionResponse(string Id, string Name, bool IsFork, DefaultBranchModel DefaultBranchRef);
}