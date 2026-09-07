// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - Staff Document Storage
// Services/StaffDocumentRepository.cs

// References:
// Microsoft Learn - Develop with Azure Blob Storage and .NET
// Microsoft Learn - BlobContainerClient
// Microsoft Learn - BlobClient

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Functions.Services
{
    /// Provides Azure Blob Storage operations for staff documents.
    public class StaffDocumentRepository : IStaffDocumentRepository
    {
        // Blob container name required for staff documents.
        private const string ContainerName = "staff-docs";

        private readonly BlobContainerClient _containerClient;

        public StaffDocumentRepository(IConfiguration configuration)
        {
            // This is separate from AzureWebJobsStorage because staff
            // documents use the configured Azure Storage account.
            string connectionString =
                configuration["StaffDocumentsStorage"]
                ?? throw new InvalidOperationException(
                    "The StaffDocumentsStorage configuration value is missing.");

            _containerClient = new BlobContainerClient(
                connectionString,
                ContainerName);
        }

        /// Creates the private staff-docs container when it does not exist.
        private async Task EnsureContainerExistsAsync(
            CancellationToken cancellationToken)
        {
            await _containerClient.CreateIfNotExistsAsync(
                PublicAccessType.None,
                cancellationToken: cancellationToken);
        }

        /// Uploads a staff document and returns its stored metadata.
        public async Task<StaffDocumentResponse> UploadAsync(
            string fileName,
            string contentType,
            Stream content,
            long contentLength,
            CancellationToken cancellationToken = default)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            BlobClient blobClient =
                _containerClient.GetBlobClient(fileName);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                },

                // Prevent an existing blob from being overwritten.
                Conditions = new BlobRequestConditions
                {
                    IfNoneMatch = ETag.All
                }
            };

            // BlobClient automatically handles uploading the stream
            // in suitable blocks when the document is larger.
            await blobClient.UploadAsync(
                content,
                uploadOptions,
                cancellationToken);

            BlobProperties properties =
                (await blobClient.GetPropertiesAsync(
                    cancellationToken: cancellationToken)).Value;

            return new StaffDocumentResponse
            {
                FileName = fileName,
                SizeInBytes = properties.ContentLength,
                LastModified = properties.LastModified,
                ContentType =
                    properties.ContentType ?? contentType
            };
        }

        /// Returns metadata for all staff documents in the container.
        public async Task<IReadOnlyList<StaffDocumentResponse>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var documents =
                new List<StaffDocumentResponse>();

            await foreach (BlobItem blob in
                _containerClient.GetBlobsAsync(
                    cancellationToken: cancellationToken))
            {
                documents.Add(
                    new StaffDocumentResponse
                    {
                        FileName = blob.Name,
                        SizeInBytes =
                            blob.Properties.ContentLength ?? 0,
                        LastModified =
                            blob.Properties.LastModified,
                        ContentType =
                            blob.Properties.ContentType
                            ?? "application/octet-stream"
                    });
            }

            return documents
                .OrderBy(document => document.FileName)
                .ToList();
        }

        /// Checks whether a document with the supplied name exists.
        public async Task<bool> ExistsAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            BlobClient blobClient =
                _containerClient.GetBlobClient(fileName);

            return (await blobClient.ExistsAsync(
                cancellationToken)).Value;
        }

        /// Opens a readable stream for an existing staff document.
        public async Task<Stream?> DownloadAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            BlobClient blobClient =
                _containerClient.GetBlobClient(fileName);

            bool exists =
                (await blobClient.ExistsAsync(
                    cancellationToken)).Value;

            if (!exists)
            {
                return null;
            }

            BlobDownloadStreamingResult download =
                (await blobClient.DownloadStreamingAsync(
                    cancellationToken: cancellationToken)).Value;

            return download.Content;
        }
    }
}