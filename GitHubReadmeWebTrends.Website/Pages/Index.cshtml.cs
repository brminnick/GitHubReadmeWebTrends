using GitHubReadmeWebTrends.Common;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubReadmeWebTrends.Website.Pages
{
    public class IndexModel : PageModel
    {
        const string _optOutText = "Opt-Out";
        const string _optInText = "Opt-In";

        readonly ILogger<IndexModel> _logger;
        readonly OptOutDatabase _optOutDatabase;
        readonly CloudAdvocateYamlService _cloudAdvocateYamlService;

        public IndexModel(OptOutDatabase optOutDatabase, CloudAdvocateYamlService cloudAdvocateYamlService, ILogger<IndexModel> logger) =>
            (_optOutDatabase, _cloudAdvocateYamlService, _logger) = (optOutDatabase, cloudAdvocateYamlService, logger);

        public string OutputText { get; private set; } = string.Empty;

        public async Task OnPostOptOutButtonClicked(string aliasInputText)
        {
            CloudAdvocateGitHubUserModel? matchingAzureAdvocate = null;

            await foreach (var advocate in _cloudAdvocateYamlService.GetAzureAdvocates().ConfigureAwait(false))
            {
                if (advocate.MicrosoftAlias.Equals(aliasInputText, StringComparison.OrdinalIgnoreCase))
                {
                    matchingAzureAdvocate = advocate;
                    break;
                }
            }

            if (matchingAzureAdvocate is null)
            {
                OutputText = "Error: Alias not Found\nEnsure `alias` field in your YAML file exists on cloud-developer-advocates: https://github.com/MicrosoftDocs/cloud-developer-advocates/tree/live/advocates";
            }
            else
            {
                var userOptOutModel = _optOutDatabase.GetAllOptOutModels().FirstOrDefault(x => x.Alias.Equals(matchingAzureAdvocate.MicrosoftAlias, StringComparison.OrdinalIgnoreCase));

                var updatedOptOutModel = userOptOutModel?.HasOptedOut switch
                {
                    true => new OptOutModel(matchingAzureAdvocate.MicrosoftAlias, false, userOptOutModel.CreatedAt, userOptOutModel.UpdatedAt),
                    false => new OptOutModel(matchingAzureAdvocate.MicrosoftAlias, true, userOptOutModel.CreatedAt, userOptOutModel.UpdatedAt),
                    _ => new OptOutModel(matchingAzureAdvocate.MicrosoftAlias, true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
                };

                var savedOptOutModel = userOptOutModel switch
                {
                    null => await _optOutDatabase.InsertOptOutModel(updatedOptOutModel).ConfigureAwait(false),
                    _ => await _optOutDatabase.PatchOptOutModel(updatedOptOutModel).ConfigureAwait(false)
                };

                OutputText = $"Success: {savedOptOutModel.Alias}'s preference has been set to {(savedOptOutModel.HasOptedOut ? _optInText : _optOutText)}";
            }
        }
    }
}
