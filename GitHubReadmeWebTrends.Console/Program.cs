using System;
using System.Threading;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Console
{
    class Program
    {
        readonly static IServiceProvider _service = CreateDIContainer(Environment.GetEnvironmentVariable("Token") ?? string.Empty);

        static async Task Main()
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));

            var advocateService = _service.GetRequiredService<AdvocateService>();
            var gitHubGraphQLService = _service.GetRequiredService<GitHubGraphQLApiService>();


            var advocates = await advocateService.GetCurrentAdvocates(cancellationTokenSource.Token).ConfigureAwait(false);

            foreach(var advocate in advocates)
            {
                var contributions = await gitHubGraphQLService.GetMicrosoftDocsContributionsCollection(advocate.GitHubUsername,
                                                                                                new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                                                                                                new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)).ConfigureAwait(false);
            }
        }

        static IServiceProvider CreateDIContainer(in string token)
        {
            //setup our DI
            var serviceCollection = new ServiceCollection();
            StartupService.ConfigureServices(serviceCollection, token);

            //configure console logging
            var serviceProvider = serviceCollection.BuildServiceProvider();
                
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
            logger.LogDebug("Starting application");

            return serviceProvider;
        }
    }
}
