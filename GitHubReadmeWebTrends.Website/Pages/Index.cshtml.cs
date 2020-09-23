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

        readonly OptOutDatabase _optOutDatabase;
        readonly ILogger<IndexModel> _logger;

        public IndexModel(OptOutDatabase optOutDatabase, ILogger<IndexModel> logger) =>
            (_optOutDatabase, _logger) = (optOutDatabase, logger);

        public string OptOutButtonText { get; private set; } = "Update Opt-Out Preference";
        public string OutputText { get; private set; } = string.Empty;

        public async Task OnPostOptOutButtonClicked(string aliasInputText)
        {
            var temp = _optOutDatabase.GetAllOptOutModels();

            var matchingAzureAdvocate = _optOutDatabase.GetOptOutModel(aliasInputText);

            if (matchingAzureAdvocate is null)
            {
                OutputText = "Error: Alias not Found\nEnsure YAML file exists on cloud-developer-advocates: https://github.com/MicrosoftDocs/cloud-developer-advocates/tree/live/advocates";
            }
            else
            {
                var userOptOutModel = _optOutDatabase.GetAllOptOutModels().FirstOrDefault(x => x.Alias.Equals(matchingAzureAdvocate.Alias, StringComparison.OrdinalIgnoreCase));

                var updatedOptOutModel = userOptOutModel?.HasOptedOut switch
                {
                    true => new OptOutModel(matchingAzureAdvocate.Alias, false, userOptOutModel.CreatedAt, userOptOutModel.UpdatedAt),
                    false => new OptOutModel(matchingAzureAdvocate.Alias, true, userOptOutModel.CreatedAt, userOptOutModel.UpdatedAt),
                    _ => new OptOutModel(matchingAzureAdvocate.Alias, true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
                };

                var savedOptOutModel = userOptOutModel switch
                {
                    null => await _optOutDatabase.InsertOptOutModel(updatedOptOutModel).ConfigureAwait(false),
                    _ => await _optOutDatabase.PatchOptOutModel(updatedOptOutModel).ConfigureAwait(false)
                };

                userOptOutModel = savedOptOutModel;

                OutputText = $"Success: {savedOptOutModel.Alias} preference has been set to {(savedOptOutModel.HasOptedOut ? _optInText : _optOutText)}";

                OptOutButtonText = savedOptOutModel switch
                {
                    { HasOptedOut: true } => _optInText,
                    _ => _optOutText
                };
            }
        }
    }


}
