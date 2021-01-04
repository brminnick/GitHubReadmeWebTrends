using System;
using AzureAdvocates.Functions;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AzureAdvocates.Functions
{
    class Startup : FunctionsStartup
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;
        static readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<CloudBlobClient>(CloudStorageAccount.Parse(_storageConnectionString).CreateCloudBlobClient());
            builder.Services.AddSingleton<BlobStorageService>();

            StartupService.ConfigureServices(builder.Services, _token);
        }
    }
}
