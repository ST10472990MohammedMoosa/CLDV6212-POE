// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - Staff Document Storage
// DTOs/StaffDocumentResponse.cs

namespace CoffeeNChill.Functions.DTOs
{
    /// <summary>
    /// Represents the document metadata returned by the
    /// CoffeeNChill staff-document endpoints.
    /// </summary>
    public class StaffDocumentResponse
    {
        /// <summary>
        /// Name of the document stored in the staff-docs File Share.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Size of the stored document in bytes.
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// Date and time when the document was last modified.
        /// </summary>
        public DateTimeOffset? LastModified { get; set; }

        /// <summary>
        /// MIME type associated with the document.
        /// </summary>
        public string ContentType { get; set; } = "application/pdf";
    }
}