// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - Staff Document Storage
// Interfaces/IStaffDocumentRepository.cs

using CoffeeNChill.Functions.DTOs;

namespace CoffeeNChill.Functions.Interfaces
{
    /// Defines the storage operations required by the
    /// CoffeeNChill staff-document Functions.
    public interface IStaffDocumentRepository
    {
        /// Uploads a new operational document to Azure File Storage.
        Task<StaffDocumentResponse> UploadAsync(
            string fileName,
            string contentType,
            Stream content,
            long contentLength,
            CancellationToken cancellationToken = default);

        /// Returns metadata for all files stored in staff-docs.
        Task<IReadOnlyList<StaffDocumentResponse>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// Checks whether a file with the supplied name already exists.
        Task<bool> ExistsAsync(
            string fileName,
            CancellationToken cancellationToken = default);

        /// Opens a readable stream for the requested document.
        /// Returns null when the document does not exist.
        Task<Stream?> DownloadAsync(
            string fileName,
            CancellationToken cancellationToken = default);
    }
}