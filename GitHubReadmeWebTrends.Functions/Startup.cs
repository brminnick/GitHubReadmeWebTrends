using System;
using GitHubReadmeWebTrends.Common;
using GitHubReadmeWebTrends.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace GitHubReadmeWebTrends.Functions
{
    public class Startup : FunctionsStartup
    {
        static readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<CloudQueueClient>(CloudStorageAccount.Parse(_storageConnectionString).CreateCloudQueueClient());

            StartupService.ConfigureServices(builder.Services);
        }
    }
}
