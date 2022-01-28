using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using GitHubReadmeWebTrends.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureAdvocates.Functions
{
    class Program
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;
        readonly static string _connectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString") ?? string.Empty;
        static readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;

        static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder => configurationBuilder.AddCommandLine(args))
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    // Add Logging
                    services.AddLogging();

                    // Add HttpClient
                    services.AddHttpClient();

                    // Add Custom Services
                    services.AddSingleton<BlobServiceClient>(new BlobServiceClient(_storageConnectionString));
                    services.AddSingleton<BlobStorageService>();

                    StartupService.ConfigureServices(services, _token, options => options.UseSqlServer(_connectionString));
                })
                .Build();

            return host.RunAsync();
        }
    }
}
