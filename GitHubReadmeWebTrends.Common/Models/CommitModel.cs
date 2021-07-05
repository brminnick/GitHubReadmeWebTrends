using System;

namespace GitHubReadmeWebTrends.Common
{
    public record CommitModel(string Sha, string Node_id, Uri Url, Uri Html_url, string Message);
}
