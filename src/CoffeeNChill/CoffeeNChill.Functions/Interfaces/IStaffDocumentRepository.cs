// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - Staff Document Storage
// Interfaces/IStaffDocumentRepository.cs

using CoffeeNChill.Functions.DTOs;

namespace CoffeeNChill.Functions.Interfaces
{
    /// <summary>
    /// Defines the storage operations required by the
    /// CoffeeNChill staff-document Functions.
    /// </summary>
    public interface IStaffDocumentRepository
    {
        /// <summary>
        /// Uploads a new operational document to Azure File Storage.
        /// </summary>
        Task<StaffDocumentResponse> UploadAsync(
            string fileName,
            string contentType,
            Stream content,
            long contentLength,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns metadata for all files stored in staff-docs.
        /// </summary>
        Task<IReadOnlyList<StaffDocumentResponse>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a file with the supplied name already exists.
        /// </summary>
        Task<bool> ExistsAsync(
            string fileName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens a readable stream for the requested document.
        /// Returns null when the document does not exist.
        /// </summary>
        Task<Stream?> DownloadAsync(
            string fileName,
            CancellationToken cancellationToken = default);
    }
}