namespace AzureAdvocates.Functions
{
    class AdovocatesTotalContributionsModel
    {
        public AdovocatesTotalContributionsModel(int totalAdvocatesWithContributions, int totalAdvocates) =>
            (TotalAdvocatesWithContributions, TotalAdvocates) = (totalAdvocatesWithContributions, totalAdvocates);

        public int TotalAdvocatesWithContributions { get; }
        public int TotalAdvocates { get; }
    }
}
