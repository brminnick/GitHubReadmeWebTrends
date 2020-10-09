using System;

namespace GitHubReadmeWebTrends.Functions
{
    public class CloudAdvocatePowerBIModel
    {
        public CloudAdvocatePowerBIModel(string name, string alias, Uri gitHub, Uri twitter, Uri linkedIn)
        {
            Name = name;
            Alias = alias;
            GitHub = gitHub;
            Twitter = twitter;
            LinkedIn = linkedIn;
        }

        public string Name { get; }
        public string Alias { get; }
        public Uri GitHub { get; }
        public Uri Twitter { get; }
        public Uri LinkedIn { get; }
    }
}
