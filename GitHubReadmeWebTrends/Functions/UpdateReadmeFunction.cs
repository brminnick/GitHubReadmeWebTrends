using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    public class UpdateReadmeFunction
    {
        [FunctionName(nameof(UpdateReadmeFunction))]
        [return: Queue(QueueConstants.VerifyWebTrendsQueue)]
        public async Task<Repository> Run([QueueTrigger(QueueConstants.RepositoriesQueue)] (Repository, CloudAdvocateGitHubUserModel) data, ILogger log)
        {
        }
}
