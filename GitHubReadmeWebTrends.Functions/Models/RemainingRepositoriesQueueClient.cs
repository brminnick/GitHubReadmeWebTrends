using System;
using Azure.Storage.Queues;

namespace GitHubReadmeWebTrends.Functions
{
    public class RemainingRepositoriesQueueClient : QueueClient
    {
        public RemainingRepositoriesQueueClient() : base(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), QueueConstants.RemainingRepositoriesQueue)
        {

        }
    }
}
