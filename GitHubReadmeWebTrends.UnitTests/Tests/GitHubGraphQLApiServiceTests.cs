using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitHubReadmeWebTrends.Common.UnitTests
{
    class GitHubGraphQLApiServiceTests : BaseTest
    {
        [Test]
        public async Task GetMicrosoftDocsContributionsCollectionTest()
        {
            // Arrange
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            // Act
            var microsoftDocsContributions = await gitHubGraphQLApiService.GetMicrosoftDocsContributionsCollection("aaronpowell",
                                                                                                                    new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                                                                                                                    new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(0, microsoftDocsContributions.TotalIssueContributions);
            Assert.AreEqual(133, microsoftDocsContributions.TotalCommitContributions);
            Assert.AreEqual(0, microsoftDocsContributions.TotalRepositoryContributions);
            Assert.AreEqual(17, microsoftDocsContributions.TotalPullRequestContributions);
            Assert.AreEqual(10, microsoftDocsContributions.TotalPullRequestReviewContributions);

            Assert.IsEmpty(microsoftDocsContributions.IssueContributionsRepositories);
            Assert.IsNotEmpty(microsoftDocsContributions.CommitContributionsRepositories);
            Assert.IsNotEmpty(microsoftDocsContributions.PullRequestContributionsRepositories);
            Assert.IsNotEmpty(microsoftDocsContributions.PullRequestReviewContributionsRepositories);
        }
    }
}