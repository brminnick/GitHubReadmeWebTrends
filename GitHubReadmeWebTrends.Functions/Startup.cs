using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;
using VerifyGitHubReadmeLinks.Functions;

[assembly: FunctionsStartup(typeof(Startup))]
namespace VerifyGitHubReadmeLinks.Functions
{
    public class Startup : FunctionsStartup
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddRefitClient<IGitHubGraphQLApiClient>()
              .ConfigureHttpClient(client =>
              {
                  client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi);
                  client.DefaultRequestHeaders.Authorization = getBearerToken();
              })
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddRefitClient<IGitHubApiClient>()
              .ConfigureHttpClient(client =>
              {
                  client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl);
                  client.DefaultRequestHeaders.Authorization = getBearerToken();
              })
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddSingleton<YamlService>();
            builder.Services.AddSingleton<OptOutDatabase>();
            builder.Services.AddSingleton<GitHubApiService>();
            builder.Services.AddSingleton<GitHubGraphQLApiService>();

            static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));

            static AuthenticationHeaderValue getBearerToken()=> new AuthenticationHeaderValue("bearer", _token);
            static DecompressionMethods getDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;
        }
    }
}
