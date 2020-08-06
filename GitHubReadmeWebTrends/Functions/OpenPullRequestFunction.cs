using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    class OpenPullRequestFunction
    {
        [FunctionName(nameof(OpenPullRequestFunction))]
        public async Task Run([QueueTrigger(QueueConstants.OpenPullRequestQueue)] (Repository,CloudAdvocateGitHubUserModel) data, ILogger log)
        {
            throw new NotImplementedException();
        }
    }
}
