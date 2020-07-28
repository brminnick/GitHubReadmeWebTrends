using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    public static class Orchestrator
    {
        [FunctionName(nameof(Orchestrator))]
        public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("VerifyGitHubReadmeLinks_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("VerifyGitHubReadmeLinks_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("VerifyGitHubReadmeLinks_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }
    }
}