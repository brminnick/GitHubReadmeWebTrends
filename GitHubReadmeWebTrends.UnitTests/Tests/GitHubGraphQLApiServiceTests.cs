using System;
using System.Threading;
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
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var advocateService = ServiceCollection.ServiceProvider.GetRequiredService<AdvocateService>();
            var gitHubGraphQLApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubGraphQLApiService>();

            // Act
            var johnPapaDocsContributionsCollection = await gitHubGraphQLApiService.GetMicrosoftDocsContributionsCollection("johnpapa",
                                                                                                                            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                                                                                                                            new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)).ConfigureAwait(false);

            // Assert
            Assert.Greater(johnPapaDocsContributionsCollection.TotalCommitContributions, 0);
        }
    }
}