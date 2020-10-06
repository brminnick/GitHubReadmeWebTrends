using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    public static class StartupService
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;

        public static void ConfigureServices(in IServiceCollection services)
        {
            services.AddLogging();

            services.AddRefitClient<IGitHubGraphQLApiClient>()
              .ConfigureHttpClient(client =>
              {
                  client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi);
                  client.DefaultRequestHeaders.Authorization = getBearerToken();
              })
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            services.AddRefitClient<IGitHubRestApiClient>()
              .ConfigureHttpClient(client =>
              {
                  client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl);
                  client.DefaultRequestHeaders.Authorization = getBearerToken();
              })
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
              .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, sleepDurationProvider));

            services.AddSingleton<YamlService>();
            services.AddSingleton<OptOutDatabase>();
            services.AddSingleton<GitHubRestApiService>();
            services.AddSingleton<GitHubGraphQLApiService>();
            services.AddSingleton<CloudAdvocateService>();

            static TimeSpan sleepDurationProvider(int attemptNumber) => TimeSpan.FromSeconds(Math.Pow(2, attemptNumber));

            static AuthenticationHeaderValue getBearerToken() => new AuthenticationHeaderValue("bearer", _token);
            static DecompressionMethods getDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;
        }
    }
}
