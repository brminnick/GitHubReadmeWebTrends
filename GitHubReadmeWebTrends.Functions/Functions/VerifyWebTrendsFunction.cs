using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    //Inspired by https://github.com/spboyer/ca-readme-tracking-links-action/
    class VerifyWebTrendsFunction
    {
        static readonly IReadOnlyList<string> _microsoftDomainsList = new[]
        {
            "microsoft.com",
            "msdn.com",
            "visualstudio.com",
            "azure.com"
        };

        //https://stackoverflow.com/a/64286141/5953643
        static readonly Regex _urlRegex = new(@"(?<!`)(`(?:`{2})?)(?:(?!\1).)*?\1|((?:ht|f)tps?://[\w-]+(?>\.[\w-]+)+(?:[\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?)", RegexOptions.Singleline);

        readonly OptOutDatabase _optOutDatabase;

        public VerifyWebTrendsFunction(OptOutDatabase optOutDatabase) => _optOutDatabase = optOutDatabase;

        [FunctionName(nameof(VerifyWebTrendsFunction))]
        public async Task Run([QueueTrigger(QueueConstants.VerifyWebTrendsQueue)] (Repository, CloudAdvocateGitHubUserModel) data, ILogger log,
                                 [Queue(QueueConstants.OpenPullRequestQueue)] ICollector<Repository> openPullRequestCollector)
        {
            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Started");

            var (repository, gitHubUser) = data;

            var updatedReadme = _urlRegex.Replace(repository.ReadmeText,
                                                    x => x.Groups[2].Success
                                                        ? UpdateUrl(x.Groups[0].Value, gitHubUser.MicrosoftTeam, "0000", gitHubUser.MicrosoftAlias)
                                                        : x.Value);

            if (!updatedReadme.Equals(repository.ReadmeText))
            {
                var optOutModel = await _optOutDatabase.GetOptOutModel(gitHubUser.MicrosoftAlias).ConfigureAwait(false);

                if (optOutModel?.HasOptedOut is true)
                {
                    log.LogInformation($"Ignoring Readme Because {gitHubUser.FullName} has opted out");
                }
                else
                {
                    log.LogInformation($"Updated Readme for {repository.Owner} {repository.Name}");
                    openPullRequestCollector.Add(new Repository(repository.Id, repository.Owner, repository.Name, repository.DefaultBranchOid, repository.DefaultBranchPrefix, repository.DefaultBranchName, repository.IsFork, updatedReadme));
                }
            }

            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Completed");
        }

        static string UpdateUrl(in string url, in string eventName, in string channel, in string alias)
        {
            foreach (var domain in _microsoftDomainsList)
            {
                //Include Microsoft Domains
                //Exclude Email Addresses, e.g. bramin@microsoft.com
                //Exclude Azure DevOps
                //Exclude Visual Studio Status Badges
                //Exclude XAML Namespace
                //Exclude CodeSpaces
                if (url.Contains(domain)
                    && !url.Contains('@')
                    && !url.ContainsWebTrendsQuery()
                    && !url.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase)
                    && !(url.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase) && url.Contains("build", StringComparison.OrdinalIgnoreCase))
                    && !(url.Contains("schemas.microsoft.com", StringComparison.OrdinalIgnoreCase) && url.Contains("xaml", StringComparison.OrdinalIgnoreCase))
                    && !url.Contains("online.visualstudio.com/environments"))
                {
                    var uriBuilder = new UriBuilder(url);

                    uriBuilder.AddWebTrendsQuery(eventName, channel, alias);
                    uriBuilder.RemoveLocale();

                    if (uriBuilder.Scheme == Uri.UriSchemeHttp)
                    {
                        var hadDefaultPort = uriBuilder.Uri.IsDefaultPort;
                        uriBuilder.Scheme = Uri.UriSchemeHttps;
                        uriBuilder.Port = hadDefaultPort ? -1 : uriBuilder.Port;
                    }

                    return uriBuilder.Uri.ToString();
                }
            }

            return url;
        }
    }

    static class UriBuilderExtensions
    {
        const string _webTrendsQueryKey = "WT.mc_id";
        static readonly Regex _localeRegex = new("^/\\w{2}-\\w{2}");

        public static bool ContainsWebTrendsQuery(this string url)
        {
            var uriBuilder = new UriBuilder(url);
            var queryStringDictionary = QueryHelpers.ParseQuery(uriBuilder.Query);

            if (queryStringDictionary.ContainsKey(_webTrendsQueryKey))
            {
                var webTrendsQuery = queryStringDictionary[_webTrendsQueryKey].First().ToString();
                var webTrendsQuerySections = webTrendsQuery.Split("-");

                //Ensure middle (second) category of the WebTrends Query is numberic
                return webTrendsQuerySections.Count() is 3
                        && long.TryParse(webTrendsQuerySections[1], out _);
            }

            return false;
        }

        public static void RemoveLocale(this UriBuilder builder) => builder.Path = _localeRegex.Replace(builder.Path, string.Empty);

        public static void AddWebTrendsQuery(this UriBuilder builder, in string team, in string devOpsId, in string alias)
        {
            var webTrendsQueryValue = $"{team}-{devOpsId}-{alias}";

            var queryStringDictionary = QueryHelpers.ParseQuery(builder.Query);
            queryStringDictionary.Remove(_webTrendsQueryKey);
            queryStringDictionary.Add(_webTrendsQueryKey, webTrendsQueryValue);

            var queryBuilder = new QueryBuilder(queryStringDictionary.SelectMany(x => x.Value, (c, v) => new KeyValuePair<string, string>(c.Key, v)).ToList());
            builder.Query = queryBuilder.ToQueryString().ToString();
        }
    }
}
