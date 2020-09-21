using System;
using System.Linq;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Website.Pages
{
    public class IndexModel : PageModel
    {
        const string _optOutText = "Opt-Out";
        const string _optInText = "Opt-In";

        readonly OptOutDatabase _optOutDatabase;
        readonly ILogger<IndexModel> _logger;

        OptOutModel? _userOptOutModel;

        public IndexModel(OptOutDatabase optOutDatabase, ILogger<IndexModel> logger) =>
            (_optOutDatabase, _logger) = (optOutDatabase, logger);

        public string OptOutText { get; private set; } = string.Empty;

        public void OnGet()
        {
            var optOutModelList = _optOutDatabase.GetAllOptOutModels();

            _userOptOutModel = optOutModelList.FirstOrDefault(x => x.Alias is "bramin");

            UpdateOptOutButtonText();
        }

        public async Task OnPostOptOutButtonClicked()
        {
            var updatedOptOutModel = _userOptOutModel?.HasOptedOut switch
            {
                true => new OptOutModel(_userOptOutModel.Id, _userOptOutModel.Alias, false, _userOptOutModel.CreatedAt, _userOptOutModel.UpdatedAt),
                false => new OptOutModel(_userOptOutModel.Id, _userOptOutModel.Alias, true, _userOptOutModel.CreatedAt, _userOptOutModel.UpdatedAt),
                _ => new OptOutModel(Guid.NewGuid(), "bramin", true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            };

            var savedOptOutModel = _userOptOutModel switch
            {
                null => await _optOutDatabase.InsertOptOutModel(updatedOptOutModel).ConfigureAwait(false),
                _ => await _optOutDatabase.PatchOptOutModel(updatedOptOutModel).ConfigureAwait(false)
            };

            _userOptOutModel = savedOptOutModel;

            UpdateOptOutButtonText();
        }

        void UpdateOptOutButtonText()
        {
            OptOutText = _userOptOutModel switch
            {
                { HasOptedOut: true } => _optInText,
                _ => _optOutText
            };
        }
    }


}
