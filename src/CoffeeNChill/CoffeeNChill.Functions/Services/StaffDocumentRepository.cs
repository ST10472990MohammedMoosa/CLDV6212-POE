// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - Staff Document Storage
// Services/StaffDocumentRepository.cs

using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Functions.Services
{
    /// <summary>
    /// Provides Azure File Share storage operations for staff documents.
    /// </summary>
    public class StaffDocumentRepository : IStaffDocumentRepository
    {
        // File Share name required by the assignment brief.
        private const string ShareName = "staff-docs";

        private readonly ShareClient _shareClient;

        public StaffDocumentRepository(IConfiguration configuration)
        {
            // This is separate from AzureWebJobsStorage because
            // staff documents use a real Azure Storage account.
            string connectionString =
                configuration["StaffDocumentsStorage"]
                ?? throw new InvalidOperationException(
                    "The StaffDocumentsStorage configuration value is missing.");

            _shareClient = new ShareClient(
                connectionString,
                ShareName);
        }

        /// <summary>
        /// Creates the staff-docs File Share when it does not already exist
        /// and returns its root directory.
        /// </summary>
        private async Task<ShareDirectoryClient> GetRootDirectoryAsync(
            CancellationToken cancellationToken)
        {
            await _shareClient.CreateIfNotExistsAsync(
                cancellationToken: cancellationToken);

            return _shareClient.GetRootDirectoryClient();
        }

        public async Task<StaffDocumentResponse> UploadAsync(
            string fileName,
            string contentType,
            Stream content,
            long contentLength,
            CancellationToken cancellationToken = default)
        {
            ShareDirectoryClient directory =
                await GetRootDirectoryAsync(cancellationToken);

            ShareFileClient fileClient =
                directory.GetFileClient(fileName);

            // Azure Files requires the file size to be created
            // before its content is uploaded.
            await fileClient.CreateAsync(
                maxSize: contentLength,
                options: new ShareFileCreateOptions
                {
                    HttpHeaders = new ShareFileHttpHeaders
                    {
                        ContentType = contentType
                    }
                },
                cancellationToken: cancellationToken);

            // Upload the supplied stream directly into the allocated file.
            await fileClient.UploadRangeAsync(
                new HttpRange(0, contentLength),
                content,
                cancellationToken: cancellationToken);

            ShareFileProperties properties =
                (await fileClient.GetPropertiesAsync(
                    cancellationToken: cancellationToken)).Value;

            return new StaffDocumentResponse
            {
                FileName = fileName,
                SizeInBytes = properties.ContentLength,
                LastModified = properties.LastModified,
                ContentType = properties.ContentType ?? contentType
            };
        }

        public async Task<IReadOnlyList<StaffDocumentResponse>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            ShareDirectoryClient directory =
                await GetRootDirectoryAsync(cancellationToken);

            var documents = new List<StaffDocumentResponse>();

            await foreach (ShareFileItem item in
                directory.GetFilesAndDirectoriesAsync(
                    cancellationToken: cancellationToken))
            {
                // Only return files, not directories.
                if (item.IsDirectory)
                {
                    continue;
                }

                ShareFileClient fileClient =
                    directory.GetFileClient(item.Name);

                ShareFileProperties properties =
                    (await fileClient.GetPropertiesAsync(
                        cancellationToken: cancellationToken)).Value;

                documents.Add(new StaffDocumentResponse
                {
                    FileName = item.Name,
                    SizeInBytes = properties.ContentLength,
                    LastModified = properties.LastModified,
                    ContentType =
                        properties.ContentType
                        ?? "application/octet-stream"
                });
            }

            return documents
                .OrderBy(document => document.FileName)
                .ToList();
        }

        public async Task<bool> ExistsAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            ShareDirectoryClient directory =
                await GetRootDirectoryAsync(cancellationToken);

            ShareFileClient fileClient =
                directory.GetFileClient(fileName);

            return (await fileClient.ExistsAsync(
                cancellationToken)).Value;
        }

        public async Task<Stream?> DownloadAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            ShareDirectoryClient directory =
                await GetRootDirectoryAsync(cancellationToken);

            ShareFileClient fileClient =
                directory.GetFileClient(fileName);

            bool exists = (await fileClient.ExistsAsync(
                cancellationToken)).Value;

            if (!exists)
            {
                return null;
            }

            ShareFileDownloadInfo download =
                (await fileClient.DownloadAsync(
                    cancellationToken: cancellationToken)).Value;

            return download.Content;
        }
    }
}