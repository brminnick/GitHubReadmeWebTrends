using System.Collections.Generic;
using System.Linq;

namespace VerifyGitHubReadmeLinks
{
    class RepositoryConnectionResponse
    {
        public RepositoryConnectionResponse(User_RepositoryConnectionResponse user)
        {
            RepositoryList = user.Repositories.RepositoryList.Select(x => x.Name).ToList();
            PageInfo = user.Repositories.PageInfo;
        }

        public List<string> RepositoryList { get; }
        public PageInfo PageInfo { get; }
    }

    class User_RepositoryConnectionResponse
    {
        public User_RepositoryConnectionResponse(Repositories_RepositoryConnectionResponse repositories) => Repositories = repositories;

        public Repositories_RepositoryConnectionResponse Repositories { get; }
    }

    class Repositories_RepositoryConnectionResponse
    {
        public Repositories_RepositoryConnectionResponse(IEnumerable<Repository_RepositoryConnectionResponse> nodes, PageInfo pageInfo) => (RepositoryList, PageInfo) = (nodes.ToList(), pageInfo);

        public List<Repository_RepositoryConnectionResponse> RepositoryList { get; }

        public PageInfo PageInfo { get; }
    }

    public class Repository_RepositoryConnectionResponse
    {
        public Repository_RepositoryConnectionResponse(string name) => Name = name;

        public string Name { get; }
    }
}
