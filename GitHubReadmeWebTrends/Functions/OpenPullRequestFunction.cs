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
            var branchName = $"AddWebTrends {DateTimeOffset.UtcNow:yyyy-MM}";

            var forkedRepository = await ForkRepository(repository).ConfigureAwait(false);

            await CreateNewBranch(forkedRepository, branchName).ConfigureAwait(false);

            await CommitUpdatedReadme(forkedRepository, branchName).ConfigureAwait(false);

            await OpenPullRequest(forkedRepository, branchName);
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

            var temp = JsonConvert.SerializeObject(updateFileContent);

            var updateFileResponse = await _gitHubApiService.UpdateFile(forkedRepository.Owner, forkedRepository.Name, forkedReameFile_AddWebTrendsBranch.Path, updateFileContent).ConfigureAwait(false);
        }

        async Task OpenPullRequest(Repository forkedRepository, string branchName)
        {
            throw new NotImplementedException();
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
                    throw new Exception($"Failed to Create New Branch: \"{branchName}\"");
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
