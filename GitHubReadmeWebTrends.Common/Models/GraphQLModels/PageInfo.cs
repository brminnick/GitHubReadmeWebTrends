﻿namespace GitHubReadmeWebTrends.Common
{
    public record PageInfo(string EndCursor, bool HasNextPage, bool HasPreviousPage, string StartCursor);
}
