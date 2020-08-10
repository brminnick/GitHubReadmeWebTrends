using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
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

            log.LogInformation($"Forked Repository for {repository.Owner} {repository.Name}");

            await CreateNewBranch(forkedRepository, branchName).ConfigureAwait(false);

            log.LogInformation($"Create New Branch for {forkedRepository.Owner} {forkedRepository.Name}");

            await CommitUpdatedReadme(forkedRepository, branchName).ConfigureAwait(false);

            log.LogInformation($"Commited Readme Updates for {forkedRepository.Owner} {forkedRepository.Name}");

            await OpenPullRequest(forkedRepository, repository, branchName).ConfigureAwait(false);

            log.LogInformation($"Open Pull Request for {forkedRepository.Owner} {forkedRepository.Name}");
        }

        async Task CommitUpdatedReadme(Repository forkedRepository, string branchName)
        {
            var forkedReadmeFile_DefaultBranch = await _gitHubApiService.GetReadme(forkedRepository.Owner, forkedRepository.Name).ConfigureAwait(false);
            var forkedReameFile_AddWebTrendsBranch = await _gitHubApiService.GetFile(forkedRepository.Owner, forkedRepository.Name, forkedReadmeFile_DefaultBranch.Path, branchName).ConfigureAwait(false);

            var currentUserInformation = await _gitHubGraphQLApiService.GetViewerInformation().ConfigureAwait(false);
#if DEBUG
            if (string.IsNullOrWhiteSpace(currentUserInformation.Viewer.Email))
                currentUserInformation = new GitHubViewerResponse(new Viewer(currentUserInformation.Viewer.Name, currentUserInformation.Viewer.Login, _backupEmailAddress));
#endif

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
            const string pullRequestBody = "TBD";

            var createPullRequestGuid = Guid.NewGuid();

            var createPullRequestResult = await _gitHubGraphQLApiService.CreatePullRequest(forkedRepository.Id, originalRepository.DefaultBranchName, branchName, "Add Web Trends", pullRequestBody, createPullRequestGuid).ConfigureAwait(false);
            if (createPullRequestResult.Result.ClientMutationId != createPullRequestGuid.ToString())
                throw new Exception($"Failed to Create New Pull Request for \"{forkedRepository.Name}\"");
        }

        async Task<Repository> ForkRepository(Repository repository)
        {
            var createForkResponse = await _gitHubApiService.CreateFork(repository.Owner, repository.Name).ConfigureAwait(false);

            var forkedRepositoryResponse = await _gitHubGraphQLApiService.GetRepository(createForkResponse.OwnerLogin, createForkResponse.Name).ConfigureAwait(false);

            return new Repository(forkedRepositoryResponse.Repository.Id,
                                    forkedRepositoryResponse.Repository.Owner,
                                    forkedRepositoryResponse.Repository.Name,
                                    forkedRepositoryResponse.Repository.DefaultBranchOid,
                                    forkedRepositoryResponse.Repository.DefaultBranchPrefix,
                                    forkedRepositoryResponse.Repository.DefaultBranchName,
                                    repository.ReadmeText);
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
