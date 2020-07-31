namespace VerifyGitHubReadmeLinks
{
    class PageInfo
    {
        public PageInfo(string endCursor, bool hasNextPage, bool hasPreviousPage, string startCursor) =>
            (EndCursor, HasNextPage, HasPreviousPage, StartCursor) = (endCursor, hasNextPage, hasPreviousPage, startCursor);

        public string EndCursor { get; }
        public bool HasNextPage { get; }
        public bool HasPreviousPage { get; }
        public string StartCursor { get; }
    }
}
