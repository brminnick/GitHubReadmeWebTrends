using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;

[assembly: FunctionsStartup(typeof(VerifyGitHubReadmeLinks.Startup))]
namespace VerifyGitHubReadmeLinks
{
    public class Startup : FunctionsStartup
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddRefitClient<IGitHubApiClient>()
              .ConfigureHttpClient(client =>
              {
                  client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl);
                  client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", _token);
              })
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            builder.Services.AddSingleton<GitHubApiService>();
            builder.Services.AddSingleton<YamlService>();

            static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
        }
    }
}
