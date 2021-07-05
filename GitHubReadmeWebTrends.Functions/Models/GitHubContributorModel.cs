using GitHubReadmeWebTrends.Common;

namespace GitHubReadmeWebTrends.Functions
{
    record GitHubContributorModel : AdvocateModel
    {
        public GitHubContributorModel(in ContributionsCollectionModel contributionsCollection, AdvocateModel advocateModel)
            : this(contributionsCollection, advocateModel.Name, advocateModel.GitHubUsername, advocateModel.MicrosoftAlias, advocateModel.Team, advocateModel.RedditUserName)
        {

        }

        public GitHubContributorModel(in ContributionsCollectionModel contributionsCollection, in string fullName, in string gitHubUserName, in string microsoftAlias, in string microsoftTeam, in string? redditUserName)
            : base(gitHubUserName, microsoftAlias, redditUserName, microsoftTeam, fullName)
        {
            Contributions = contributionsCollection;
        }

        public ContributionsCollectionModel Contributions { get; }
    }
}
