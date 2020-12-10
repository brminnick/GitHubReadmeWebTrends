using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using GitHubApiStatus.Extensions;
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
                    client.DefaultRequestHeaders.Authorization = getBearerTokenHeader();
                })
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = HttpConfigurationService.GetDecompressionMethods() })
                .AddPolicyHandler(HttpConfigurationService.GetPolicyHandler());


            services.AddRefitClient<IGitHubRestApiClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl);
                    client.DefaultRequestHeaders.Authorization = getBearerTokenHeader();
                })
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = HttpConfigurationService.GetDecompressionMethods() })
                .AddPolicyHandler(HttpConfigurationService.GetPolicyHandler());

            services.AddGitHubApiStatusService(getBearerTokenHeader(), new ProductHeaderValue(nameof(GitHubReadmeWebTrends)))
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = HttpConfigurationService.GetDecompressionMethods() })
                .AddPolicyHandler(HttpConfigurationService.GetPolicyHandler());

            services.AddSingleton<YamlService>();
            services.AddSingleton<OptOutDatabase>();
            services.AddSingleton<GitHubRestApiService>();
            services.AddSingleton<GitHubGraphQLApiService>();
            services.AddSingleton<CloudAdvocateService>();

            static AuthenticationHeaderValue getBearerTokenHeader() => new("bearer", _token);
        }
    }
}