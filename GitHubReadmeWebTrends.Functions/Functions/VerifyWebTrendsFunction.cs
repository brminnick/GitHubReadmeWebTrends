using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
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

        [Function(nameof(VerifyWebTrendsFunction)), QueueOutput(QueueConstants.OpenPullRequestQueue)]
        public async Task<IReadOnlyList<Repository>> Run([QueueTrigger(QueueConstants.VerifyWebTrendsQueue)] RepositoryAdvocateModel data, FunctionContext context)
        {
            var log = context.GetLogger<VerifyWebTrendsFunction>();
            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Started");

            var (repository, gitHubUser) = data;

            var openPullRequestList = new List<Repository>();

            var updatedReadme = _urlRegex.Replace(repository.ReadmeText,
                                                    x => x.Groups[2].Success
                                                        ? UpdateUrl(x.Groups[0].Value, gitHubUser.Team, "0000", gitHubUser.MicrosoftAlias)
                                                        : x.Value);

            if (!updatedReadme.Equals(repository.ReadmeText))
            {
                var optOutModel = await _optOutDatabase.GetOptOutModel(gitHubUser.MicrosoftAlias).ConfigureAwait(false);

                if (optOutModel?.HasOptedOut is true)
                {
                    log.LogInformation($"Ignoring Readme Because {gitHubUser.Name} has opted out");
                }
                else
                {
                    log.LogInformation($"Updated Readme for {repository.Owner} {repository.Name}");
                    openPullRequestList.Add(new Repository(repository.Id, repository.Owner, repository.Name, repository.DefaultBranchOid, repository.DefaultBranchPrefix, repository.DefaultBranchName, repository.IsFork, updatedReadme));
                }
            }

            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Completed");

            return openPullRequestList;
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
                return webTrendsQuerySections.Length is 3
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
