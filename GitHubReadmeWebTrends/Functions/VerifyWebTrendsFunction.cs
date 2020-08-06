using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    //Inspired by https://github.com/spboyer/ca-readme-tracking-links-action/
    static class VerifyWebTrendsFunction
    {
        static readonly IReadOnlyList<Regex> _defaultDomainsRegexList = new[]
        {
            new Regex("(.*\\.)?microsoft\\.com$", RegexOptions.IgnoreCase),
            new Regex("(.*\\.)?msdn\\.com$", RegexOptions.IgnoreCase),
            new Regex("(.*\\.)?visualstudio\\.com$", RegexOptions.IgnoreCase)
        };

        [FunctionName(nameof(GetReadmeFunction))]
        public static void Run([QueueTrigger(QueueConstants.VerifyWebTrendsQueue)] (Repository repository, CloudAdvocateGitHubUserModel gitHubUserModel) data, ILogger log,
                                        [Queue(QueueConstants.UpdateReadmeFunction)] ICollector<Repository> openPullRequestCollector)
        {
            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Started");

            var (repository, gitHubUser) = data;

            var pattern = @"(((http|ftp|https):\/\/)?[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:\/~\+#]*[\w\-\@?^=%&amp;\/~\+#])?)";
            var regex = new Regex(pattern);

            var updatedReadme = regex.Replace(repository.ReadmeText, x => AppendTrackingInfo(x.Groups[0].Value, repository.Name, "GitHub", gitHubUser.MicrosoftAlias));

            if (!string.IsNullOrWhiteSpace(updatedReadme))
            {
                log.LogInformation($"Updated Readme for {repository.Owner} {repository.Name}");
                openPullRequestCollector.Add(new Repository(repository.Owner, repository.Name, updatedReadme));
            }

            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Completed");
        }

        static UriBuilder RemoveLocale(this UriBuilder builder)
        {
            var localeMatcher = new Regex("^/\\w{2}-\\w{2}");
            builder.Path = localeMatcher.Replace(builder.Path, string.Empty);

            return builder;
        }

        static string AppendTrackingInfo(in string link, in string eventName, in string channel, in string alias)
        {
            foreach (var regex in _defaultDomainsRegexList)
            {
                if (regex.IsMatch(link))
                {
                    var uriBuilder = new UriBuilder(link);

                    uriBuilder = uriBuilder.AddTrackingCode(eventName, channel, alias);
                    uriBuilder = uriBuilder.RemoveLocale();

                    return uriBuilder.Uri.ToString();
                }
            }

            return link;
        }

        static UriBuilder AddTrackingCode(this UriBuilder builder, string eventName, string channel, string alias)
        {
            const string trackingName = "WT.mc_id";
            string trackingCode = $"{eventName}-{channel}-{alias}";

            var queryStringDictionary = QueryHelpers.ParseQuery(builder.Query);
            queryStringDictionary.Remove(trackingName);
            queryStringDictionary.Add(trackingName, trackingCode);

            var queryBuilder = new QueryBuilder(queryStringDictionary.SelectMany(x => x.Value, (c, v) => new KeyValuePair<string, string>(c.Key, v)).ToList());
            builder.Query = queryBuilder.ToQueryString().ToString();

            return builder;
        }
    }
}
