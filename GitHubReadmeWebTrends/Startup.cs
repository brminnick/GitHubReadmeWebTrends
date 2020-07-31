using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;

[assembly: FunctionsStartup(typeof(VerifyGitHubReadmeLinks.Startup))]
namespace VerifyGitHubReadmeLinks
{
    public class Startup : FunctionsStartup
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddHttpClient<GitHubApiService>()
                .ConfigureHttpClient(client => client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token))
                .ConfigureHttpClient(client => client.DefaultRequestHeaders.UserAgent.ParseAdd(nameof(VerifyGitHubReadmeLinks)))
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(1, sleepDurationProvider));

            builder.Services.AddSingleton<GitHubApiService>();

            static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
        }
    }
}
