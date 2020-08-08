using System.Collections.Generic;
using System.Linq;

namespace VerifyGitHubReadmeLinks
{
    class RepositoryConnectionResponse
    {
        public RepositoryConnectionResponse(User_RepositoryConnectionResponse user)
        {
            foreach (var repository in user.Repositories.RepositoryList)
            {
                if (!repository.IsFork && !RepositoryList.Any(x => x.Name == repository.Name && x.Owner == user.Login))
                    RepositoryList.Add(new Repository(repository.Id, user.Login, repository.Name, repository.DefaultBranch));
            }

            PageInfo = user.Repositories.PageInfo;
        }

        public List<Repository> RepositoryList { get; } = Enumerable.Empty<Repository>().ToList();
        public PageInfo PageInfo { get; }
    }

    class User_RepositoryConnectionResponse
    {
        public User_RepositoryConnectionResponse(string login, Repositories_RepositoryConnectionResponse repositories) =>
            (Login, Repositories) = (login, repositories);

        public Repositories_RepositoryConnectionResponse Repositories { get; }
        public string Login { get; }
    }

    class Repositories_RepositoryConnectionResponse
    {
        public Repositories_RepositoryConnectionResponse(IEnumerable<Repository_RepositoryConnectionResponse> nodes, PageInfo pageInfo) => (RepositoryList, PageInfo) = (nodes.ToList(), pageInfo);

        public List<Repository_RepositoryConnectionResponse> RepositoryList { get; }

        public PageInfo PageInfo { get; }
    }

    class Repository_RepositoryConnectionResponse
    {
        public Repository_RepositoryConnectionResponse(string id, string name, bool isFork, DefaultBranchModel defaultBranchRef) =>
            (Id, Name, IsFork, DefaultBranch) = (id, name, isFork, defaultBranchRef);

        public string Id { get; }
        public string Name { get; }
        public bool IsFork { get; }
        public DefaultBranchModel DefaultBranch { get; }
    }
}
