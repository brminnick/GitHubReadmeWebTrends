using System.Collections.Generic;
using System.Linq;

namespace GitHubReadmeWebTrends.Common
{
    public record RepositoriesConnectionResponse(User_RepositoriesConnectionResponse User)
    {
        public IReadOnlyList<Repository> RepositoryList => GetRepositoryList();
        public PageInfo PageInfo => User.Repositories.PageInfo;

        IReadOnlyList<Repository> GetRepositoryList()
        {
            var repositoryList = new List<Repository>();

            foreach (var repository in User.Repositories.Nodes)
            {
                if (repository.Owner.Login == User.Login && !repository.IsFork && repository.DefaultBranchRef != null && !repositoryList.Any(x => x.Name == repository.Name && x.Owner == User.Login))
                    repositoryList.Add(new Repository(repository.Id, User.Login, repository.Name, repository.DefaultBranchRef, repository.IsFork));
            }

            return repositoryList;
        }
    }

    public record User_RepositoriesConnectionResponse(string Login, Repositories_RepositoriesConnectionResponse Repositories);

    public record Repositories_RepositoriesConnectionResponse(IEnumerable<Repository_RepositoriesConnectionResponse> Nodes, PageInfo PageInfo);

    public record Repository_RepositoriesConnectionResponse(string Id, string Name, bool IsFork, Owner Owner, DefaultBranchModel DefaultBranchRef);

    public record Owner_RepositoriesConnectionResponse(string Login);
}
