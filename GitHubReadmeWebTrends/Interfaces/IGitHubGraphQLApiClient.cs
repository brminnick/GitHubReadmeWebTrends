using System.Threading.Tasks;
using GraphQL;
using Refit;

namespace VerifyGitHubReadmeLinks
{
    [Headers("User-Agent: " + nameof(VerifyGitHubReadmeLinks))]
    interface IGitHubGraphQLApiClient
    {
        [Post("")]
        Task<GraphQLResponse<RepositoryConnectionResponse>> RepositoryConnectionQuery([Body] RepositoryConnectionQueryContent request);
    }

    class RepositoryConnectionQueryContent : GraphQLRequest
    {
        public RepositoryConnectionQueryContent(string repositoryOwner, string endCursorString, int numberOfRepositoriesPerRequest = 100)
            : base("query {  user(login:" + repositoryOwner + ")  { repositories(first:" + numberOfRepositoriesPerRequest + endCursorString + ") { nodes { name }, pageInfo { endCursor, hasNextPage, hasPreviousPage, startCursor } } } }")
        {

        }
    }
}
