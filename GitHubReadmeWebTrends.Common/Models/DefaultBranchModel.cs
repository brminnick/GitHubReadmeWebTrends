namespace GitHubReadmeWebTrends.Common
{
    public record DefaultBranchModel(string Id, string Name, string Prefix, Target Target);

    public record Target(string Oid);
}
