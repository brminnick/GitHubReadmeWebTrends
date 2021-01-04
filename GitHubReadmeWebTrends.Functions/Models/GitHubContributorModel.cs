using GitHubReadmeWebTrends.Common;

namespace GitHubReadmeWebTrends.Functions
{
    class GitHubContributorModel : CloudAdvocateGitHubUserModel
    {
        public GitHubContributorModel(in ContributionsCollectionModel contributionsCollection, CloudAdvocateGitHubUserModel cloudAdvocateGitHubUserModel)
            : this(contributionsCollection, cloudAdvocateGitHubUserModel.FullName, cloudAdvocateGitHubUserModel.GitHubUserName, cloudAdvocateGitHubUserModel.MicrosoftAlias, cloudAdvocateGitHubUserModel.MicrosoftTeam)
        {

        }

        public GitHubContributorModel(in ContributionsCollectionModel contributionsCollection, in string fullName, in string gitHubUserName, in string microsoftAlias, in string microsoftTeam)
            : base(fullName, gitHubUserName, microsoftAlias, microsoftTeam)
        {
            Contributions = contributionsCollection;
        }

        public ContributionsCollectionModel Contributions { get; }
    }
}
