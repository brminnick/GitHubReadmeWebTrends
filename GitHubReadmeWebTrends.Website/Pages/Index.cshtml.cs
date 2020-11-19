using System;
using System.Linq;
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
        readonly GraphServiceClient _graphServiceClient;
        readonly CloudAdvocateService _cloudAdvocateYamlService;

        public IndexModel(OptOutDatabase optOutDatabase, CloudAdvocateService cloudAdvocateYamlService, GraphServiceClient graphServiceClient, ILogger<IndexModel> logger)
        {
            _logger = logger;
            _optOutDatabase = optOutDatabase;
            _graphServiceClient = graphServiceClient;
            _cloudAdvocateYamlService = cloudAdvocateYamlService;
        }

        public string LoggedInLabelText { get; private set; } = string.Empty;
        public string CurrentPreferenceText { get; private set; } = string.Empty;
        public string ButtonText { get; private set; } = string.Empty;
        public string OutputText { get; private set; } = string.Empty;

        public async Task OnGet()
        {
            var microsoftAlias = await GetCurrentUserAlias().ConfigureAwait(false);
            var userOptOutModel = GetOptOutModel(microsoftAlias);

            UpdateButtonText(userOptOutModel);
            UpdateLoggedInLabelText(microsoftAlias);
            UpdateCurrentPreferenceText(userOptOutModel);
        }

        public async Task OnPostOptOutButtonClicked()
        {
            CloudAdvocateGitHubUserModel? matchingAzureAdvocate = null;
            var microsoftAlias = await GetCurrentUserAlias().ConfigureAwait(false);

            await foreach (var advocate in _cloudAdvocateYamlService.GetAzureAdvocates().ConfigureAwait(false))
            {
                if (advocate.MicrosoftAlias.Equals(microsoftAlias, StringComparison.OrdinalIgnoreCase))
                {
                    matchingAzureAdvocate = advocate;
                    break;
                }
            }

            if (matchingAzureAdvocate is null)
            {
                OutputText = $"Error: Alias not Found\nEnsure your login, {microsoftAlias}, matches the `alias` field in your YAML file on the cloud-developer-advocates repo: https://github.com/MicrosoftDocs/cloud-developer-advocates/tree/live/advocates";

                UpdateButtonText(null);
                UpdateCurrentPreferenceText(null);
            }
            else
            {
                var userOptOutModel = GetOptOutModel(microsoftAlias);
                var updatedOptOutModel = userOptOutModel?.HasOptedOut switch
                {
                    true => new OptOutModel(matchingAzureAdvocate.MicrosoftAlias, false, userOptOutModel.CreatedAt, DateTimeOffset.UtcNow),
                    false => new OptOutModel(matchingAzureAdvocate.MicrosoftAlias, true, userOptOutModel.CreatedAt, DateTimeOffset.UtcNow),
                    _ => new OptOutModel(matchingAzureAdvocate.MicrosoftAlias, true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
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

        OptOutModel GetOptOutModel(string microsoftAlias) => _optOutDatabase.GetAllOptOutModels().FirstOrDefault(x => x.Alias.Equals(microsoftAlias, StringComparison.OrdinalIgnoreCase));

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
