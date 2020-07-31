using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    class VerifyWebTrendsFunction
    {
        public async Task Run([QueueTrigger(QueueConstants.VerifyWebTrendsQueue)] Repository repository, ILogger log)
        {
            
        }
    }
}
