using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
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
              .AddPolicyHandler(getPolicyHandler());


            services.AddRefitClient<IGitHubRestApiClient>()
              .ConfigureHttpClient(client =>
              {
                  client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl);
                  client.DefaultRequestHeaders.Authorization = getBearerToken();
              })
              .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = getDecompressionMethods() })
              .AddPolicyHandler(getPolicyHandler());

            services.AddSingleton<YamlService>();
            services.AddSingleton<OptOutDatabase>();
            services.AddSingleton<GitHubRestApiService>();
            services.AddSingleton<GitHubGraphQLApiService>();
            services.AddSingleton<CloudAdvocateService>();
            services.AddSingleton<GitHubApiStatusService>();

            static AuthenticationHeaderValue getBearerToken() => new AuthenticationHeaderValue("bearer", _token);
            static DecompressionMethods getDecompressionMethods() => DecompressionMethods.Deflate | DecompressionMethods.GZip;
            static IAsyncPolicy<HttpResponseMessage> getPolicyHandler() => HttpPolicyExtensions.HandleTransientHttpError().OrResult(msg => msg.StatusCode is HttpStatusCode.Forbidden).WaitAndRetryAsync(10, count => TimeSpan.FromSeconds(60));
        }
    }
}
