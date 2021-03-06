﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using GitHubApiStatus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    public static class StartupService
    {
        public static void ConfigureServices(in IServiceCollection services, string gitHubToken, in Action<DbContextOptionsBuilder> dbContextOptionsAction)
        {
            // Logging
            services.AddLogging();

            // Database
            services.AddDbContext<OptOutDbContext>(dbContextOptionsAction);
            services.AddScoped<OptOutDatabase>();

            // Refit
            services.AddRefitClient<IGitHubGraphQLApiClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(GitHubConstants.GitHubGraphQLApi);
                    client.DefaultRequestHeaders.Authorization = getBearerTokenHeader(gitHubToken);
                })
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = HttpConfigurationService.GetDecompressionMethods() })
                .AddPolicyHandler(HttpConfigurationService.GetPolicyHandler());


            services.AddRefitClient<IGitHubRestApiClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(GitHubConstants.GitHubRestApiUrl);
                    client.DefaultRequestHeaders.Authorization = getBearerTokenHeader(gitHubToken);
                })
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = HttpConfigurationService.GetDecompressionMethods() })
                .AddPolicyHandler(HttpConfigurationService.GetPolicyHandler());

            services.AddRefitClient<IAdvocateApi>()
                        .ConfigureHttpClient(client => client.BaseAddress = new Uri(AdvocateConstants.BaseAdvocateApi))
                        .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler { AutomaticDecompression = HttpConfigurationService.GetDecompressionMethods() })
                        .AddPolicyHandler(HttpConfigurationService.GetPolicyHandler());

            services.AddGitHubApiStatusService(getBearerTokenHeader(gitHubToken), new ProductHeaderValue(nameof(GitHubReadmeWebTrends)))
                .ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler { AutomaticDecompression = HttpConfigurationService.GetDecompressionMethods() })
                .AddPolicyHandler(HttpConfigurationService.GetPolicyHandler());

            // Singletons
            services.AddSingleton<AdvocateService>();
            services.AddSingleton<GitHubRestApiService>();
            services.AddSingleton<GitHubGraphQLApiService>();

            static AuthenticationHeaderValue getBearerTokenHeader(in string token) => new("bearer", token);
        }
    }
}