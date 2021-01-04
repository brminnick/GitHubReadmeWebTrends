using System;
using AzureAdvocates.Functions;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AzureAdvocates.Functions
{
    class Startup : FunctionsStartup
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;

        public override void Configure(IFunctionsHostBuilder builder) => StartupService.ConfigureServices(builder.Services, _token);
    }
}
