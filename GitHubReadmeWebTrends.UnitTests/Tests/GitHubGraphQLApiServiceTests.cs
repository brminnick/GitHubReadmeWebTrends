using System;
using System.Collections.Generic;
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
            Assert.AreEqual(17, microsoftDocsContributions.TotalPullRequestContributions);
            Assert.AreEqual(10, microsoftDocsContributions.TotalPullRequestReviewContributions);

            Assert.IsEmpty(microsoftDocsContributions.IssueContributionsByRepository);
            Assert.IsNotEmpty(microsoftDocsContributions.CommitContributionsByRepository);
            Assert.IsNotEmpty(microsoftDocsContributions.PullRequestContributionsByRepository);
            Assert.IsNotEmpty(microsoftDocsContributions.PullRequestReviewContributionsByRepository);
        }

        [Test]
        public async Task GetMicrosoftLearnPullRequestsTest()
        {
            // Arrange
            var microsoftLearnPullRequests = new List<RepositoryPullRequest>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            // Act
            await foreach (var pullRequests in gitHubGraphQLApiService.GetMicrosoftLearnPullRequests().ConfigureAwait(false))
            {
                microsoftLearnPullRequests.AddRange(pullRequests);
            }

            // Assert
            Assert.IsNotEmpty(microsoftLearnPullRequests);

            foreach (var pullRequest in microsoftLearnPullRequests)
            {
                Assert.IsNotNull(pullRequest);

                if (pullRequest.Author is not null)
                    Assert.IsFalse(string.IsNullOrWhiteSpace(pullRequest.Author.Login));

                Assert.IsFalse(string.IsNullOrWhiteSpace(pullRequest.BaseRefName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(pullRequest.Id));
                Assert.IsFalse(string.IsNullOrWhiteSpace(pullRequest.RepositoryName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(pullRequest.RepositoryOwner));

                Assert.AreNotEqual(default(DateTimeOffset), pullRequest.CreatedAt);
                Assert.AreNotEqual(default(DateTimeOffset), pullRequest.MergedAt);

                Assert.IsTrue(Uri.IsWellFormedUriString(pullRequest.Url.ToString(), UriKind.Absolute));
            }
        }

        [Test]
        public async Task GetRepositoriesTest()
        {
            // Arrange
            var repositoryList = new List<Repository>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            // Act
            await foreach (var repositories in gitHubGraphQLApiService.GetRepositories("brminnick").ConfigureAwait(false))
            {
                repositoryList.AddRange(repositories);
            }

            // Assert
            Assert.IsNotEmpty(repositoryList);

            foreach (var repository in repositoryList)
            {
                Assert.IsNotNull(repository);

                Assert.IsFalse(string.IsNullOrWhiteSpace(repository.DefaultBranchName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(repository.DefaultBranchOid));
                Assert.IsFalse(string.IsNullOrWhiteSpace(repository.DefaultBranchPrefix));
                Assert.IsFalse(string.IsNullOrWhiteSpace(repository.Id));
                Assert.IsFalse(string.IsNullOrWhiteSpace(repository.Name));
                Assert.IsFalse(string.IsNullOrWhiteSpace(repository.Owner));

                Assert.IsTrue(string.IsNullOrWhiteSpace(repository.ReadmeText));
            }
        }

        [Test]
        public async Task GetRepositoryTest()
        {
            // Arrange
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            // Act
            var repository = await gitHubGraphQLApiService.GetRepository("brminnick", nameof(GitHubReadmeWebTrends)).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(repository);

            Assert.IsFalse(string.IsNullOrWhiteSpace(repository?.DefaultBranchName));
            Assert.IsFalse(string.IsNullOrWhiteSpace(repository?.DefaultBranchOid));
            Assert.IsFalse(string.IsNullOrWhiteSpace(repository?.DefaultBranchPrefix));
            Assert.IsFalse(string.IsNullOrWhiteSpace(repository?.Id));
            Assert.IsFalse(string.IsNullOrWhiteSpace(repository?.Name));
            Assert.IsFalse(string.IsNullOrWhiteSpace(repository?.Owner));

            Assert.IsTrue(string.IsNullOrWhiteSpace(repository?.ReadmeText));
        }

        [Test]
        public async Task GetDefaultBranchPullRequestsTest()
        {
            // Arrange
            var pullRequests = new List<PullRequest>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            // Act
            await foreach (var pullRequestsResult in gitHubGraphQLApiService.GetDefaultBranchPullRequests(nameof(GitHubReadmeWebTrends), "brminnick").ConfigureAwait(false))
            {
                pullRequests.AddRange(pullRequestsResult);
            }

            // Assert
            Assert.IsNotEmpty(pullRequests);

            foreach (var pullRequest in pullRequests)
            {
                if (pullRequest.Author is not null)
                    Assert.IsFalse(string.IsNullOrWhiteSpace(pullRequest.Author.Login));

                Assert.IsFalse(string.IsNullOrWhiteSpace(pullRequest.BaseRefName));
                Assert.IsFalse(string.IsNullOrWhiteSpace(pullRequest.Id));                

                Assert.AreNotEqual(default(DateTimeOffset), pullRequest.CreatedAt);
                Assert.AreNotEqual(default(DateTimeOffset), pullRequest.MergedAt);

                Assert.IsTrue(Uri.IsWellFormedUriString(pullRequest.Url.ToString(), UriKind.Absolute));
            }
        }
    }
}