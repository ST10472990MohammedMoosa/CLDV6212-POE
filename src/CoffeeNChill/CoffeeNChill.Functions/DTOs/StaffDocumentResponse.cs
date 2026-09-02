// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - Staff Document Storage
// DTOs/StaffDocumentResponse.cs
//
// Microsoft Learn - Azure Functions .NET isolated worker:

namespace CoffeeNChill.Functions.DTOs
{
    /// Represents the document metadata returned by the
    /// CoffeeNChill staff-document endpoints.
    public class StaffDocumentResponse
    {
        /// Name of the document stored in the staff-docs File Share.
        public string FileName { get; set; } = string.Empty;

        /// Size of the stored document in bytes.
        public long SizeInBytes { get; set; }

        /// Date and time when the document was last modified.
        public DateTimeOffset? LastModified { get; set; }

        /// MIME type associated with the document.00
        public string ContentType { get; set; } = "application/pdf";
    }
}