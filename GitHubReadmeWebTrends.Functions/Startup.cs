using GitHubReadmeWebTrends.Common;
using GitHubReadmeWebTrends.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace GitHubReadmeWebTrends.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder) => StartupService.ConfigureServices(builder.Services);
    }
}
