﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace GitHubReadmeWebTrends.Website.Pages
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class IndexModel : PageModel
    {
        const string _optOutText = "Opt-Out";
        const string _optInText = "Opt-In";

        readonly ILogger<IndexModel> _logger;
        readonly OptOutDatabase _optOutDatabase;
        readonly AdvocateService _advocateService;
        readonly GraphServiceClient _graphServiceClient;

        public IndexModel(ILogger<IndexModel> logger,
                            OptOutDatabase optOutDatabase,
                            AdvocateService advocateService,
                            GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _optOutDatabase = optOutDatabase;
            _advocateService = advocateService;
            _graphServiceClient = graphServiceClient;
        }

        public string LoggedInLabelText { get; private set; } = string.Empty;
        public string CurrentPreferenceText { get; private set; } = string.Empty;
        public string ButtonText { get; private set; } = string.Empty;
        public string OutputText { get; private set; } = string.Empty;

        public async Task OnGet()
        {
            var microsoftAlias = await GetCurrentUserAlias().ConfigureAwait(false);
            var userOptOutModel = await _optOutDatabase.GetOptOutModel(microsoftAlias).ConfigureAwait(false);

            UpdateButtonText(userOptOutModel);
            UpdateLoggedInLabelText(microsoftAlias);
            UpdateCurrentPreferenceText(userOptOutModel);
        }

        public async Task OnPostOptOutButtonClicked()
        {
            _logger.LogInformation("Opt Out Button Clicked");


            var microsoftAlias = await GetCurrentUserAlias().ConfigureAwait(false);

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var currentAdvocates = await _advocateService.GetCurrentAdvocates(cancellationTokenSource.Token).ConfigureAwait(false);
            var matchingAdvocate = currentAdvocates.SingleOrDefault(x => x.MicrosoftAlias == microsoftAlias);

            if (matchingAdvocate is null)
            {
                OutputText = $"Error: Alias not Found\nEnsure your login, {microsoftAlias}, matches the `alias` field in your YAML file on the cloud-developer-advocates repo: https://github.com/MicrosoftDocs/cloud-developer-advocates/tree/live/advocates";

                UpdateButtonText(null);
                UpdateCurrentPreferenceText(null);
            }
            else
            {
                var userOptOutModel = await _optOutDatabase.GetOptOutModel(microsoftAlias).ConfigureAwait(false);
                var updatedOptOutModel = userOptOutModel?.HasOptedOut switch
                {
                    true => new OptOutModel(matchingAdvocate.MicrosoftAlias, false, userOptOutModel.CreatedAt, DateTimeOffset.UtcNow),
                    false => new OptOutModel(matchingAdvocate.MicrosoftAlias, true, userOptOutModel.CreatedAt, DateTimeOffset.UtcNow),
                    _ => new OptOutModel(matchingAdvocate.MicrosoftAlias, true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
                };

                var savedOptOutModel = userOptOutModel switch
                {
                    null => await _optOutDatabase.InsertOptOutModel(updatedOptOutModel).ConfigureAwait(false),
                    _ => await _optOutDatabase.PatchOptOutModel(updatedOptOutModel).ConfigureAwait(false)
                };

                OutputText = $"Success! {savedOptOutModel.Alias}'s GitHub Readme Preference has been set to {(savedOptOutModel.HasOptedOut ? _optOutText : _optInText)}";

                UpdateButtonText(savedOptOutModel);
                UpdateCurrentPreferenceText(savedOptOutModel);
            }

            UpdateLoggedInLabelText(microsoftAlias);
        }

        async Task<string> GetCurrentUserAlias()
        {
            var user = await _graphServiceClient.Me.Request().GetAsync().ConfigureAwait(false);

            var email = user.UserPrincipalName;
            return email?.Split('@')[0] ?? throw new NullReferenceException();
        }

        void UpdateLoggedInLabelText(in string microsoftAlias) => LoggedInLabelText = $"Logged in as {microsoftAlias}@microsoft.com";

        void UpdateCurrentPreferenceText(in OptOutModel? userOptOutModel) => CurrentPreferenceText = userOptOutModel switch
        {
            { HasOptedOut: true } => $"Current Preference: {_optOutText}",
            _ => $"Current Preference: {_optInText}"
        };

        void UpdateButtonText(in OptOutModel? userOptOutModel) => ButtonText = userOptOutModel switch
        {
            { HasOptedOut: true } => "Opt Back In",
            _ => "Opt Out"
        };
    }
}
