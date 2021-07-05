using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitHubReadmeWebTrends.Common.UnitTests
{
    class GitHubRestApiServiceTests : BaseTest
    {
        [Test]
        public async Task GetReadmeTest()
        {
            // Arrange
            var gitHubRestApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubRestApiService>();

            // Act
            var repositoryFile = await gitHubRestApiService.GetReadme("brminnick", nameof(GitHubReadmeWebTrends));

            // Assert
            Assert.IsNotNull(repositoryFile);

            Assert.IsTrue(Uri.IsWellFormedUriString(repositoryFile.Download_Url?.ToString(), UriKind.Absolute));

            Assert.AreEqual("README.md", repositoryFile.Name);
            Assert.AreEqual("README.md", repositoryFile.Path);

            Assert.IsFalse(string.IsNullOrWhiteSpace(repositoryFile.Sha));
        }

        [Test]
        public async Task GetFileTest()
        {
            // Arrange
            var gitHubRestApiService = ServiceCollection.ServiceProvider.GetRequiredService<GitHubRestApiService>();

            // Act
            var repositoryFile = await gitHubRestApiService.GetFile("brminnick", nameof(GitHubReadmeWebTrends), "README.md", "main");

            // Assert
            Assert.IsNotNull(repositoryFile);

            Assert.IsTrue(Uri.IsWellFormedUriString(repositoryFile.Download_Url?.ToString(), UriKind.Absolute));

            Assert.AreEqual("README.md", repositoryFile.Name);
            Assert.AreEqual("README.md", repositoryFile.Path);
            Assert.IsFalse(string.IsNullOrWhiteSpace(repositoryFile.Sha));
        }
    }
}
