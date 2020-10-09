using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Console
{
    class Program
    {
        readonly ILogger _log = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(Console));

        static void Main(string[] args)
        {

        }
    }
}
