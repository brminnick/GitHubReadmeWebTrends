using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace AzureAdvocates.Functions
{
    class BlobStorageService
    {
        const string _microsoftLearnContributionsContainerName = "cloudadvocatemicrosoftlearncontributions";
        readonly CloudBlobClient _blobClient;

        public BlobStorageService(CloudBlobClient cloudBlobClient) => _blobClient = cloudBlobClient;

        public Task UploadCloudAdvocateMicrosoftLearnContributions(IEnumerable<CloudAdvocateGitHubContributorModel> azureDataCenterIpRangeModel, string blobName)
        {
            var container = GetBlobContainer(_microsoftLearnContributionsContainerName);
            var blob = container.GetBlockBlobReference(blobName);

            return blob.UploadTextAsync(JsonConvert.SerializeObject(azureDataCenterIpRangeModel));
        }

        public async Task<IReadOnlyList<CloudAdvocateGitHubContributorModel>> GetCloudAdvocateMicrosoftLearnContributors()
        {
            var blobList = new List<CloudBlockBlob>();
            await foreach (var blob in GetBlobs<CloudBlockBlob>(_microsoftLearnContributionsContainerName).ConfigureAwait(false))
            {
                blobList.Add(blob);
            }

            var gitHubContributorListBlob = blobList.OrderByDescending(x => x.Properties.Created).First();
            var serializedGitHubContributorList = await gitHubContributorListBlob.DownloadTextAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<IReadOnlyList<CloudAdvocateGitHubContributorModel>>(serializedGitHubContributorList) ?? throw new NullReferenceException();
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
