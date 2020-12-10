using System;
using System.Net.Http;
using System.Net.Http.Headers;
using GitHubReadmeWebTrends.Common;
using GitHubReadmeWebTrends.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.DependencyInjection;
using Refit;

[assembly: FunctionsStartup(typeof(Startup))]
namespace GitHubReadmeWebTrends.Functions
{
    public class Startup : FunctionsStartup
    {
        static readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;
        readonly static string _token = Environment.GetEnvironmentVariable("Token_brminnick") ?? string.Empty;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddRefitClient<IGitHubGraphQLApiPrivateRepoClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi);
                    client.DefaultRequestHeaders.Authorization = getBearerTokenHeader();
                })
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = HttpConfigurationService.GetDecompressionMethods() })
                .AddPolicyHandler(HttpConfigurationService.GetPolicyHandler());

            builder.Services.AddSingleton<CloudQueueClient>(CloudStorageAccount.Parse(_storageConnectionString).CreateCloudQueueClient());
            builder.Services.AddSingleton<GitHubGraphQLApiService_PrivateRepoAccess>();

            StartupService.ConfigureServices(builder.Services);

            static AuthenticationHeaderValue getBearerTokenHeader() => new("bearer", _token);
        }
    }
}
