using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace AzureAdvocates.Functions
{
    class BlobStorageService
    {
        const string _microsoftLearnContributionsContainerName = "cloudadvocatemicrosoftlearncontributions";

        readonly BlobServiceClient _blobClient;
        readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(BlobServiceClient cloudBlobClient, ILogger<BlobStorageService> logger) =>
            (_blobClient, _logger) = (cloudBlobClient, logger);

        public Task UploadCloudAdvocateMicrosoftLearnContributions(IEnumerable<CloudAdvocateGitHubContributorModel> cloudAdvocateGitHubContributorModel, string blobName)
        {
            var container = GetBlobContainerClient(_microsoftLearnContributionsContainerName);
            var blobUri = new Uri($"{container.Uri}/{blobName}");

            var blobClient = new BlobClient(blobUri);
            var blobContent = JsonSerializer.SerializeToUtf8Bytes<IEnumerable<CloudAdvocateGitHubContributorModel>>(cloudAdvocateGitHubContributorModel);

            return blobClient.UploadAsync(new BinaryData(blobContent));
        }

        public async Task<IReadOnlyList<CloudAdvocateGitHubContributorModel>> GetCloudAdvocateMicrosoftLearnContributors()
        {
            var blobList = new List<BlobItem>();
            await foreach (var blob in GetBlobs(_microsoftLearnContributionsContainerName).ConfigureAwait(false))
            {
                blobList.Add(blob);
                _logger.LogInformation($"Found {blob.Name}");
            }

            var gitHubContributorListBlob = blobList.OrderByDescending(x => x.Properties.CreatedOn).First();
            _logger.LogInformation($"Most Recent Blob: {gitHubContributorListBlob.Name}");

            var blobClient = new BlobClient(new Uri($"{GetBlobContainerClient(_microsoftLearnContributionsContainerName).Uri}/{gitHubContributorListBlob.Name}"));
            var blobContentResponse = await blobClient.DownloadContentAsync().ConfigureAwait(false);

            return JsonSerializer.Deserialize<IReadOnlyList<CloudAdvocateGitHubContributorModel>>(blobContentResponse.Value.Content) ?? throw new NullReferenceException();
        }

        async IAsyncEnumerable<BlobItem> GetBlobs(string containerName)
        {
            var blobContainerClient = GetBlobContainerClient(containerName);

            await foreach (var blob in blobContainerClient.GetBlobsAsync().ConfigureAwait(false))
            {
                if (blob is not null)
                    yield return blob;
            }
        }

        BlobContainerClient GetBlobContainerClient(string containerName) => _blobClient.GetBlobContainerClient(containerName);
    }
}
