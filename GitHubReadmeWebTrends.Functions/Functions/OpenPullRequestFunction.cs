using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Refit;
using GitHubReadmeWebTrends.Common;

namespace GitHubReadmeWebTrends.Functions
{
    class OpenPullRequestFunction
    {
        readonly static string _backupEmailAddress = Environment.GetEnvironmentVariable("BackupEmailAddress") ?? string.Empty;

        readonly GitHubApiService _gitHubApiService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public OpenPullRequestFunction(GitHubApiService gitHubApiService, GitHubGraphQLApiService gitHubGraphQLApiService) =>
            (_gitHubApiService, _gitHubGraphQLApiService) = (gitHubApiService, gitHubGraphQLApiService);

        [FunctionName(nameof(OpenPullRequestFunction))]
        public async Task Run([QueueTrigger(QueueConstants.OpenPullRequestQueue)] Repository repository, ILogger log)
        {
            var branchName = $"AddWebTrends-{DateTimeOffset.UtcNow:yyyy-MM}";

            var forkedRepository = await ForkRepository(repository).ConfigureAwait(false);
            if (forkedRepository is null)
                return;

            log.LogInformation($"Forked Repository for {repository.Owner} {repository.Name}");

            await CreateNewBranch(forkedRepository, branchName).ConfigureAwait(false);

            log.LogInformation($"Create New Branch for {forkedRepository.Owner} {forkedRepository.Name}");

            await CommitUpdatedReadme(forkedRepository, branchName).ConfigureAwait(false);

            log.LogInformation($"Commited Readme Updates for {forkedRepository.Owner} {forkedRepository.Name}");

            try
            {
                await OpenPullRequest(forkedRepository, repository, branchName).ConfigureAwait(false);

                log.LogInformation($"Opened Pull Request from {forkedRepository.Owner} {forkedRepository.Name} to {repository.Owner} {repository.Name}");
            }
            catch (ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.Conflict)
            {
                //If a Pull Request with the same name is already open, GitHubGraphQLApiService.CreatePullRequest may return a 409 Conflict
            }
            catch (Exception e) when (e.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                //If a Pull Request with the same name is already open, GitHubGraphQLApiService.CreatePullRequest may return "A pull request already exists..."
            }
        }

        async Task CommitUpdatedReadme(Repository forkedRepository, string branchName)
        {
            var forkedReadmeFile_DefaultBranch = await _gitHubApiService.GetReadme(forkedRepository.Owner, forkedRepository.Name).ConfigureAwait(false);
            var forkedReameFile_AddWebTrendsBranch = await _gitHubApiService.GetFile(forkedRepository.Owner, forkedRepository.Name, forkedReadmeFile_DefaultBranch.Path, branchName).ConfigureAwait(false);

            var currentUserInformation = await _gitHubGraphQLApiService.GetViewerInformation().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(currentUserInformation.Viewer.Email))
                currentUserInformation = new GitHubViewerResponse(new Viewer(currentUserInformation.Viewer.Name, currentUserInformation.Viewer.Login, _backupEmailAddress));

            var readmeAsBase64String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(forkedRepository.ReadmeText));
            var updateFileContent = new UpdateFileContentModel("Added Web Trends",
                                                                new Committer(currentUserInformation.Viewer.Name, currentUserInformation.Viewer.Email),
                                                                readmeAsBase64String,
                                                                forkedReameFile_AddWebTrendsBranch.Sha,
                                                                branchName);

            var updateFileResponse = await _gitHubApiService.UpdateFile(forkedRepository.Owner, forkedRepository.Name, forkedReameFile_AddWebTrendsBranch.Path, updateFileContent).ConfigureAwait(false);
        }

        async Task OpenPullRequest(Repository forkedRepository, Repository originalRepository, string branchName)
        {
            var createPullRequestGuid = Guid.NewGuid();

            var createPullRequestResult = await _gitHubGraphQLApiService.CreatePullRequest(originalRepository.Id, originalRepository.DefaultBranchName, $"{forkedRepository.Owner}:{branchName}", branchName, PullRequestConstants.BodyText, createPullRequestGuid).ConfigureAwait(false);
            if (createPullRequestResult.Result.ClientMutationId != createPullRequestGuid.ToString())
                throw new Exception($"Failed to Create New Pull Request for \"{forkedRepository.Name}\"");
        }

        async Task<Repository?> ForkRepository(Repository repository)
        {
            var currentUserInfo = await _gitHubGraphQLApiService.GetViewerInformation().ConfigureAwait(false);

            var forkExists = await doesForkExist(currentUserInfo.Viewer.Login, repository.Name).ConfigureAwait(false);
            if (forkExists)
                await _gitHubApiService.DeleteRepository(currentUserInfo.Viewer.Login, repository.Name).ConfigureAwait(false);

            var createForkResponse = await _gitHubApiService.CreateFork(repository.Owner, repository.Name).ConfigureAwait(false);

            var forkedRepositoryResponse = await _gitHubGraphQLApiService.GetRepository(createForkResponse.OwnerLogin, createForkResponse.Name).ConfigureAwait(false);
            if (forkedRepositoryResponse.Repository is null)
                return null;

            return new Repository(forkedRepositoryResponse.Repository.Id,
                                    forkedRepositoryResponse.Repository.Owner,
                                    forkedRepositoryResponse.Repository.Name,
                                    forkedRepositoryResponse.Repository.DefaultBranchOid,
                                    forkedRepositoryResponse.Repository.DefaultBranchPrefix,
                                    forkedRepositoryResponse.Repository.DefaultBranchName,
                                    repository.IsFork,
                                    repository.ReadmeText);

            async Task<bool> doesForkExist(string owner, string repositoryName)
            {
                var repositoryResponse = await _gitHubGraphQLApiService.GetRepository(owner, repositoryName).ConfigureAwait(false);

                return repositoryResponse.Repository != null && repositoryResponse.Repository.IsFork;
            }
        }

        async Task CreateNewBranch(Repository repository, string branchName)
        {
            const string alreadyExistsExceptionMessage = "already exists in the repository";

            try
            {
                var createBranchGiud = Guid.NewGuid();
                var createBranchResult = await _gitHubGraphQLApiService.CreateBranch(repository.Id, repository.DefaultBranchPrefix + branchName, repository.DefaultBranchOid, createBranchGiud).ConfigureAwait(false);

                if (createBranchResult.Result.ClientMutationId != createBranchGiud.ToString())
                    throw new Exception($"Failed to Create New Branch for \"{repository.Name}\"");
            }
            catch (Exception e) when (e.Message.Contains(alreadyExistsExceptionMessage))
            {
                //Todo Create New Branch
            }
            catch (AggregateException e) when (e.InnerExceptions.Any(x => x.Message.Contains(alreadyExistsExceptionMessage)))
            {
                //Todo Create New Branch
            }
        }
    }
}
