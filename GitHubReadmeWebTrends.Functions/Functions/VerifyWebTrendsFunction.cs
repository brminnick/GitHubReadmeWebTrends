using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GitHubReadmeWebTrends.Common;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    //Inspired by https://github.com/spboyer/ca-readme-tracking-links-action/
    public static class VerifyWebTrendsFunction
    {
        const string _webTrendsQueryKey = "WT.mc_id";

        static readonly IReadOnlyList<string> _microsoftDomainsList = new[]
        {
            "microsoft.com",
            "msdn.com",
            "visualstudio.com",
            "azure.com"
        };

        //https://stackoverflow.com/a/64286141/5953643
        static readonly Regex _urlRegex = new Regex(@"(?<!`)(`(?:`{2})?)(?:(?!\1).)*?\1|((?:ht|f)tps?:\/\/[\w-]+(?>\.[\w-]+)+(?:[\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?)");
        static readonly Regex _localeRegex = new Regex("^/\\w{2}-\\w{2}");

        [FunctionName(nameof(VerifyWebTrendsFunction))]
        public static void Run([QueueTrigger(QueueConstants.VerifyWebTrendsQueue)] (Repository, CloudAdvocateGitHubUserModel) data, ILogger log,
                                 [Queue(QueueConstants.OpenPullRequestQueue)] ICollector<Repository> openPullRequestCollector)
        {
            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Started");

            var (repository, gitHubUser) = data;

            var updatedReadme = _urlRegex.Replace(repository.ReadmeText,
                                                    x => x.Groups[2].Success
                                                        ? UpdateUrl(x.Groups[0].Value, repository.Name.Replace(".", "").Replace("-", "").ToLower(), "github", gitHubUser.MicrosoftAlias)
                                                        : x.Value);

            if (!updatedReadme.Equals(repository.ReadmeText))
            {
                log.LogInformation($"Updated Readme for {repository.Owner} {repository.Name}");
                openPullRequestCollector.Add(new Repository(repository.Id, repository.Owner, repository.Name, repository.DefaultBranchOid, repository.DefaultBranchPrefix, repository.DefaultBranchName, repository.IsFork, updatedReadme));
            }

            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Completed");
        }

        static string UpdateUrl(in string link, in string eventName, in string channel, in string alias)
        {
            foreach (var domain in _microsoftDomainsList)
            {
                //Include Microsoft Domains
                //Exclude Email Addresses, e.g. bramin@microsoft.com
                //Exclue existing WebTrends queries
                //Exclude Azure DevOps
                //Exclude Visual Studio Status Badges
                //Exclude XAML Namespace
                //Exclude CodeSpaces
                if (link.Contains(domain)
                    && !link.Contains('@')
                    && !link.Contains(_webTrendsQueryKey, StringComparison.OrdinalIgnoreCase)
                    && !link.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase)
                    && !(link.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase) && link.Contains("build", StringComparison.OrdinalIgnoreCase))
                    && !(link.Contains("schemas.microsoft.com", StringComparison.OrdinalIgnoreCase) && link.Contains("xaml", StringComparison.OrdinalIgnoreCase))
                    && !link.Contains("online.visualstudio.com/environments"))
                {
                    var uriBuilder = new UriBuilder(link);

                    uriBuilder.AddWebTrendsQuery(eventName, channel, alias);
                    uriBuilder.RemoveLocale();

                    if (uriBuilder.Scheme is "http")
                    {
                        var hadDefaultPort = uriBuilder.Uri.IsDefaultPort;
                        uriBuilder.Scheme = Uri.UriSchemeHttps;
                        uriBuilder.Port = hadDefaultPort ? -1 : uriBuilder.Port;
                    }

                    return uriBuilder.Uri.ToString();
                }
            }

            return link;
        }

        static void RemoveLocale(this UriBuilder builder) => builder.Path = _localeRegex.Replace(builder.Path, string.Empty);

        static void AddWebTrendsQuery(this UriBuilder builder, in string eventName, in string channel, in string alias)
        {
            var webTrendsQueryValue = $"{eventName}-{channel}-{alias}";

            var queryStringDictionary = QueryHelpers.ParseQuery(builder.Query);
            queryStringDictionary.Remove(_webTrendsQueryKey);
            queryStringDictionary.Add(_webTrendsQueryKey, webTrendsQueryValue);

            var queryBuilder = new QueryBuilder(queryStringDictionary.SelectMany(x => x.Value, (c, v) => new KeyValuePair<string, string>(c.Key, v)).ToList());
            builder.Query = queryBuilder.ToQueryString().ToString();
        }
    }
}
