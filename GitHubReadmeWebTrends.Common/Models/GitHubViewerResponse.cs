namespace GitHubReadmeWebTrends.Common
{
    public record Viewer(string Name, string Login, string Email);

    public record GitHubViewerResponse(Viewer Viewer);
}
