using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubReadmeWebTrends.Common
{
    public class AdvocateService
    {
        readonly HttpClient _httpClient;
        readonly IAdvocateApi _advocateApiClient;

        public AdvocateService(IAdvocateApi advocateApiClient, IHttpClientFactory httpClientFactory) =>
            (_httpClient, _advocateApiClient) = (httpClientFactory.CreateClient(), advocateApiClient);

        public Task<IReadOnlyList<AdvocateModel>> GetCurrentAdvocates(CancellationToken cancellationToken) => _advocateApiClient.GetCurrentAdvocates(cancellationToken);

        public async IAsyncEnumerable<string> GetRedditUsernames([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var advocates = await GetCurrentAdvocates(cancellationToken).ConfigureAwait(false);

            foreach (var advocate in advocates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrWhiteSpace(advocate.RedditUserName))
                    yield return advocate.RedditUserName;
            }
        }

        public async Task<IReadOnlyList<AdvocateModel>> GetFriendsOfAdvocates(CancellationToken cancellationToken)
        {
            return await _httpClient.GetFromJsonAsync<IReadOnlyList<AdvocateModel>>("https://raw.githubusercontent.com/jamesmontemagno/team/main/team.json", cancellationToken).ConfigureAwait(false) ?? throw new JsonException();
        }
    }

}
