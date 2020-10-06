using GitHubReadmeWebTrends.Common;
using GitHubReadmeWebTrends.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace GitHubReadmeWebTrends.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<RemainingRepositoriesQueueClient>();
            StartupService.ConfigureServices(builder.Services);
        }
    }
}
