﻿using System;
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
        const string _webTrendsQueryKey = "WT.mc_id";

        static readonly IReadOnlyList<string> _microsoftDomainsList = new[]
        {
            "microsoft.com",
            "msdn.com",
            "visualstudio.com"
        };

        static readonly Regex _regex = new Regex(@"(((http|ftp|https):\/\/)?[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:\/~\+#]*[\w\-\@?^=%&amp;\/~\+#])?)");

        [FunctionName(nameof(VerifyWebTrendsFunction))]
        public static void Run([QueueTrigger(QueueConstants.VerifyWebTrendsQueue)] (Repository, CloudAdvocateGitHubUserModel) data, ILogger log,
                                        [Queue(QueueConstants.OpenPullRequestQueue)] ICollector<Repository> openPullRequestCollector)
        {
            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Started");

            var (repository, gitHubUser) = data;

            var updatedReadme = _regex.Replace(repository.ReadmeText, x => AppendTrackingInfo(x.Groups[0].Value, repository.Name.Replace("-", "").ToLower(), "github", gitHubUser.MicrosoftAlias));

            if (!updatedReadme.Equals(repository.ReadmeText))
            {
                log.LogInformation($"Updated Readme for {repository.Owner} {repository.Name}");
                openPullRequestCollector.Add(new Repository(repository.Id, repository.Owner, repository.Name, repository.DefaultBranchOid, repository.DefaultBranchPrefix, repository.DefaultBranchName, updatedReadme));
            }

            log.LogInformation($"{nameof(VerifyWebTrendsFunction)} Completed");
        }

        static string AppendTrackingInfo(in string link, in string eventName, in string channel, in string alias)
        {
            foreach (var domain in _microsoftDomainsList)
            {
                if (link.Contains(domain) && !link.Contains('@') && !link.Contains(_webTrendsQueryKey, StringComparison.OrdinalIgnoreCase))
                {
                    var uriBuilder = new UriBuilder(link);

                    AddTrackingCode(uriBuilder, eventName, channel, alias);
                    RemoveLocale(uriBuilder);

                    if (uriBuilder.Scheme is "http")
                        uriBuilder.Scheme = "https";

                    return uriBuilder.Uri.ToString();
                }
            }

            return link;
        }

        static void RemoveLocale(in UriBuilder builder)
        {
            var localeMatcher = new Regex("^/\\w{2}-\\w{2}");
            builder.Path = localeMatcher.Replace(builder.Path, string.Empty);
        }

        static void AddTrackingCode(in UriBuilder builder, in string eventName, in string channel, in string alias)
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