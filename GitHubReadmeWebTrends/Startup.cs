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
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddRefitClient<IGitHubAuthApi>()
              .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://github.com"))
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(1, sleepDurationProvider));

            builder.Services.AddSingleton<GitHubAuthService>();

            static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));
        }
    }
}
