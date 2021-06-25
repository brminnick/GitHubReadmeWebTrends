using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    public interface IAdvocateApi
    {
        [Get("/Advocates")]
        public Task<IReadOnlyList<AdvocateModel>> GetCurrentAdvocates(CancellationToken cancellationToken);

        [Get("/DashboardAdvocates")]
        public Task<IReadOnlyList<AdvocateModel>> GetDashboardAdvocates(CancellationToken cancellationToken);
    }
}
