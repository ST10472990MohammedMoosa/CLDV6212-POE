// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - Staff Document Validation
// Functions/Documents/StaffDocumentValidator.cs
//
// Microsoft Learn - Azure Functions .NET isolated worker:

namespace CoffeeNChill.Functions.Functions.Documents
{
    /// Provides reusable validation for uploaded and downloaded
    /// CoffeeNChill staff documents.
    public static class StaffDocumentValidator
    {
        // Maximum permitted document size: 10 megabytes.
        public const long MaximumFileSize =
            10 * 1024 * 1024;

        /// Validates the supplied document file name.
        public static string? ValidateFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "A file name is required.";
            }

            string safeFileName = Path.GetFileName(fileName);

            // Prevent clients from supplying directory paths.
            if (!string.Equals(
                fileName,
                safeFileName,
                StringComparison.Ordinal))
            {
                return "The file name contains an invalid path.";
            }

            if (!string.Equals(
                Path.GetExtension(safeFileName),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Only PDF staff documents are permitted.";
            }

            return null;
        }

        /// Validates the uploaded file name, type and size.
        public static string? ValidateUpload(
            string? fileName,
            string? contentType,
            long fileLength)
        {
            string? fileNameError =
                ValidateFileName(fileName);

            if (fileNameError != null)
            {
                return fileNameError;
            }

            if (fileLength <= 0)
            {
                return "The uploaded document cannot be empty.";
            }

            if (fileLength > MaximumFileSize)
            {
                return "The uploaded document cannot exceed 10 MB.";
            }

            if (!string.Equals(
                contentType,
                "application/pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                return "The uploaded document must use the application/pdf content type.";
            }

            return null;
        }

        /// Checks that the stream begins with the standard PDF signature.
        /// This prevents a renamed non-PDF file from passing validation.
        public static async Task<bool> HasPdfSignatureAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            byte[] expectedSignature =
            {
                (byte)'%',
                (byte)'P',
                (byte)'D',
                (byte)'F',
                (byte)'-'
            };

            byte[] actualSignature =
                new byte[expectedSignature.Length];

            int bytesRead = await stream.ReadAsync(
                actualSignature.AsMemory(
                    0,
                    actualSignature.Length),
                cancellationToken);

            // Reset the stream so the complete file can still be uploaded.
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return bytesRead == expectedSignature.Length
                && actualSignature.SequenceEqual(
                    expectedSignature);
        }
    }
}