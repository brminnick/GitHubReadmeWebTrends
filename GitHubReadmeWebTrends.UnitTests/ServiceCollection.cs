using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubReadmeWebTrends.Common.UnitTests
{
    public class ServiceCollection
    {
        readonly static string _token = Environment.GetEnvironmentVariable("Token") ?? string.Empty;

        readonly static Lazy<IServiceProvider> _serviceProviderHolder = new(() => CreateContainer());

        public static IServiceProvider ServiceProvider => _serviceProviderHolder.Value;

        ServiceCollection()
        {
        }

        static IServiceProvider CreateContainer()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            StartupService.ConfigureServices(services, _token, options => options.UseInMemoryDatabase("InMemoryDb"));

            return services.BuildServiceProvider();
        }
    }
}
