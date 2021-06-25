using System;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GitHubReadmeWebTrends.Functions
{
    class Program
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;
        static readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;

        static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>configurationBuilder.AddCommandLine(args))
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    // Add Logging
                    services.AddLogging();

                    // Add HttpClient
                    services.AddHttpClient();

                    // Add Custom Services
                    services.AddSingleton<CloudQueueClient>(CloudStorageAccount.Parse(_storageConnectionString).CreateCloudQueueClient());
                    StartupService.ConfigureServices(services, _token);
                })
                .Build();

            return host.RunAsync();
        }
    }
}
