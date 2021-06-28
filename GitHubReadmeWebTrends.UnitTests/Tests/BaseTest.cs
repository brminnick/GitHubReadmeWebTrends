using System.Threading.Tasks;
using NUnit.Framework;

namespace GitHubReadmeWebTrends.Common.UnitTests
{
    class BaseTest
    {
        [SetUp]
        public Task Setup() => Task.CompletedTask;
    }
}
