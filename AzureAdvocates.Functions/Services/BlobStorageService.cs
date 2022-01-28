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

        readonly BlobServiceClient _blobServiceClient;
        readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(BlobServiceClient blobClient, ILogger<BlobStorageService> logger) =>
            (_blobServiceClient, _logger) = (blobClient, logger);

        public Task UploadCloudAdvocateMicrosoftLearnContributions(IEnumerable<CloudAdvocateGitHubContributorModel> cloudAdvocateGitHubContributorModel, string blobName) =>
            UploadValue(cloudAdvocateGitHubContributorModel, blobName, _microsoftLearnContributionsContainerName);

        public Task<IReadOnlyList<CloudAdvocateGitHubContributorModel>> GetCloudAdvocateMicrosoftLearnContributors() =>
            GetLatestValue<IReadOnlyList<CloudAdvocateGitHubContributorModel>>(_microsoftLearnContributionsContainerName);

        async IAsyncEnumerable<BlobItem> GetBlobs(string containerName)
        {
            var blobContainerClient = GetBlobContainerClient(containerName);

            await foreach (var blob in blobContainerClient.GetBlobsAsync().ConfigureAwait(false))
            {
                if (blob is not null)
                    yield return blob;
            }
        }

        async Task UploadValue<T>(T data, string blobName, string containerName)
        {
            var containerClient = GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(new BinaryData(data)).ConfigureAwait(false);
        }

        async Task<T> GetLatestValue<T>(string containerName)
        {
            var blobList = new List<BlobItem>();
            await foreach (var blob in GetBlobs(containerName).ConfigureAwait(false))
            {
                blobList.Add(blob);
            }

            var newestBlob = blobList.OrderByDescending(x => x.Properties.CreatedOn).First();

            var blobClient = GetBlobContainerClient(containerName).GetBlobClient(newestBlob.Name);
            var blobContentResponse = await blobClient.DownloadContentAsync().ConfigureAwait(false);

            var serializedBlobContents = blobContentResponse.Value.Content;

            return JsonSerializer.Deserialize<T>(serializedBlobContents) ?? throw new NullReferenceException();
        }

        BlobContainerClient GetBlobContainerClient(string containerName) => _blobServiceClient.GetBlobContainerClient(containerName);
    }
}
