using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Storage.Blob;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AzureAdvocates.Functions
{
    class BlobStorageService
    {
        const string _microsoftLearnContributionsContainerName = "cloudadvocatemicrosoftlearncontributions";

        readonly CloudBlobClient _blobClient;
        readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(CloudBlobClient cloudBlobClient, ILogger<BlobStorageService> logger) =>
            (_blobClient, _logger) = (cloudBlobClient, logger);

        public Task UploadCloudAdvocateMicrosoftLearnContributions(IEnumerable<CloudAdvocateGitHubContributorModel> cloudAdvocateGitHubContributorModel, string blobName)
        {
            var container = GetBlobContainer(_microsoftLearnContributionsContainerName);
            var blob = container.GetBlockBlobReference(blobName);

            return blob.UploadTextAsync(JsonSerializer.Serialize(cloudAdvocateGitHubContributorModel));
        }

        public async Task<IReadOnlyList<CloudAdvocateGitHubContributorModel>> GetCloudAdvocateMicrosoftLearnContributors()
        {
            var blobList = new List<CloudBlockBlob>();
            await foreach (var blob in GetBlobs<CloudBlockBlob>(_microsoftLearnContributionsContainerName).ConfigureAwait(false))
            {
                blobList.Add(blob);
                _logger.LogInformation($"Found {blob.Name}");
            }

            var gitHubContributorListBlob = blobList.OrderByDescending(x => x.Properties.Created).First();
            var serializedGitHubContributorList = await gitHubContributorListBlob.DownloadTextAsync().ConfigureAwait(false);

            _logger.LogInformation($"Most Recent Blob: {serializedGitHubContributorList}");

            return JsonSerializer.Deserialize<IReadOnlyList<CloudAdvocateGitHubContributorModel>>(serializedGitHubContributorList) ?? throw new NullReferenceException();
        }

        async IAsyncEnumerable<T> GetBlobs<T>(string containerName, string prefix = "", int? maxresultsPerQuery = null, BlobListingDetails blobListingDetails = BlobListingDetails.None) where T : ICloudBlob
        {
            var blobContainer = GetBlobContainer(containerName);

            BlobContinuationToken? continuationToken = null;

            do
            {
                var response = await blobContainer.ListBlobsSegmentedAsync(prefix, true, blobListingDetails, maxresultsPerQuery, continuationToken, null, null).ConfigureAwait(false);
                continuationToken = response?.ContinuationToken;

                var blobListFromResponse = response?.Results?.OfType<T>() ?? Enumerable.Empty<T>();

                foreach (var blob in blobListFromResponse)
                {
                    yield return blob;
                }

            } while (continuationToken != null);
        }

        CloudBlobContainer GetBlobContainer(string containerName) => _blobClient.GetContainerReference(containerName);
    }
}
