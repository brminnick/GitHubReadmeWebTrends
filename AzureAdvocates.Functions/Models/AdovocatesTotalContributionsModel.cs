using System.Collections.Generic;
using System.Linq;

namespace AzureAdvocates.Functions
{
    class AdovocatesTotalContributionsModel
    {
        public AdovocatesTotalContributionsModel(int totalAdvocates,
                                                    int totalAdvocatesWithContributions,
                                                    IDictionary<string, int> totalTeamContributions)
        {
            TotalAdvocates = totalAdvocates;
            TotalAdvocatesWithContributions = totalAdvocatesWithContributions;
            TotalTeamContributions = new Dictionary<string, int>(totalTeamContributions);
        }

        public int TotalAdvocates { get; }
        public int TotalAdvocatesWithContributions { get; }
        public IReadOnlyDictionary<string, int> TotalTeamContributions { get; }
    }
}
