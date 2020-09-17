using System.Collections.Generic;
using System.Linq;

namespace VerifyGitHubReadmeLinks.Functions
{
    class RepositoriesConnectionResponse
    {
        public RepositoriesConnectionResponse(User_RepositoriesConnectionResponse user)
        {
            var repositoryList = new List<Repository>();

            foreach (var repository in user.Repositories.RepositoryList)
            {
                if (!repository.IsFork && !repositoryList.Any(x => x.Name == repository.Name && x.Owner == user.Login))
                    repositoryList.Add(new Repository(repository.Id, user.Login, repository.Name, repository.DefaultBranch));
            }

            RepositoryList = repositoryList;
            PageInfo = user.Repositories.PageInfo;
        }

        public IReadOnlyList<Repository> RepositoryList { get; }
        public PageInfo PageInfo { get; }
    }

    class User_RepositoriesConnectionResponse
    {
        public User_RepositoriesConnectionResponse(string login, Repositories_RepositoriesConnectionResponse repositories) =>
            (Login, Repositories) = (login, repositories);

        public Repositories_RepositoriesConnectionResponse Repositories { get; }
        public string Login { get; }
    }

    class Repositories_RepositoriesConnectionResponse
    {
        public Repositories_RepositoriesConnectionResponse(IEnumerable<Repository_RepositoriesConnectionResponse> nodes, PageInfo pageInfo) =>
            (RepositoryList, PageInfo) = (nodes.ToList(), pageInfo);

        public List<Repository_RepositoriesConnectionResponse> RepositoryList { get; }

        public PageInfo PageInfo { get; }
    }

    class Repository_RepositoriesConnectionResponse
    {
        public Repository_RepositoriesConnectionResponse(string id, string name, bool isFork, DefaultBranchModel defaultBranchRef) =>
            (Id, Name, IsFork, DefaultBranch) = (id, name, isFork, defaultBranchRef);

        public string Id { get; }
        public string Name { get; }
        public bool IsFork { get; }
        public DefaultBranchModel DefaultBranch { get; }
    }
}
