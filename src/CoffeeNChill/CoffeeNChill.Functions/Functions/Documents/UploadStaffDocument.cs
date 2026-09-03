// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - Staff Document Upload
// Functions/Documents/UploadStaffDocument.cs
//
// Microsoft Learn - Azure Functions .NET isolated worker:


using Azure;
using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions.Documents
{
    /// Uploads operational PDF documents to the
    /// CoffeeNChill staff-docs Azure File Share.
    public class UploadStaffDocument
    {
        private readonly IStaffDocumentRepository _repository;
        private readonly ILogger<UploadStaffDocument> _logger;

        public UploadStaffDocument(
            IStaffDocumentRepository repository,
            ILogger<UploadStaffDocument> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [Function("UploadStaffDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "post",
                Route = "documents/upload")]
            HttpRequest req)
        {
            _logger.LogInformation(
                "Staff document upload request received.");

            try
            {
                // The upload must be submitted through Postman
                // using Body -> form-data.
                if (!req.HasFormContentType)
                {
                    return CreateValidationError(
                        "The request must use multipart/form-data.");
                }

                IFormCollection form = await req.ReadFormAsync(
                    req.HttpContext.RequestAborted);

                // Postman's file field must be named "file".
                IFormFile? file = form.Files.GetFile("file");

                if (file == null)
                {
                    return CreateValidationError(
                        "Attach a PDF using the multipart form-data field named 'file'.");
                }

                string safeFileName =
                    Path.GetFileName(file.FileName);

                string? validationError =
                    StaffDocumentValidator.ValidateUpload(
                        file.FileName,
                        file.ContentType,
                        file.Length);

                if (validationError != null)
                {
                    _logger.LogWarning(
                        "Upload validation failed for {FileName}: {ValidationError}",
                        safeFileName,
                        validationError);

                    return CreateValidationError(
                        validationError);
                }

                await using Stream fileStream =
                    file.OpenReadStream();

                // Validate the actual content rather than trusting
                // only the extension and MIME type.
                bool hasValidPdfSignature =
                    await StaffDocumentValidator.HasPdfSignatureAsync(
                        fileStream,
                        req.HttpContext.RequestAborted);

                if (!hasValidPdfSignature)
                {
                    _logger.LogWarning(
                        "Upload rejected because {FileName} does not contain a valid PDF signature.",
                        safeFileName);

                    return CreateValidationError(
                        "The uploaded file content is not a valid PDF document.");
                }

                // Prevent an existing document from being overwritten.
                bool alreadyExists =
                    await _repository.ExistsAsync(
                        safeFileName,
                        req.HttpContext.RequestAborted);

                if (alreadyExists)
                {
                    _logger.LogWarning(
                        "Upload conflict: {FileName} already exists.",
                        safeFileName);

                    return new ConflictObjectResult(
                        new ErrorResponse
                        {
                            Error = "DOCUMENT_ALREADY_EXISTS",
                            Message =
                                "A document with the same file name already exists."
                        });
                }

                StaffDocumentResponse uploadedDocument =
                    await _repository.UploadAsync(
                        safeFileName,
                        "application/pdf",
                        fileStream,
                        file.Length,
                        req.HttpContext.RequestAborted);

                _logger.LogInformation(
                    "Staff document {FileName} uploaded successfully. Size: {SizeInBytes} bytes.",
                    uploadedDocument.FileName,
                    uploadedDocument.SizeInBytes);

                // Return 201 Created with the new file's metadata.
                return new ObjectResult(uploadedDocument)
                {
                    StatusCode =
                        StatusCodes.Status201Created
                };
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(
                    ex,
                    "Azure Files rejected the document upload. Status: {Status}.",
                    ex.Status);

                return CreateStorageError();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unexpected document upload error occurred.");

                return CreateStorageError();
            }
        }

        private static BadRequestObjectResult CreateValidationError(
            string message)
        {
            return new BadRequestObjectResult(
                new ErrorResponse
                {
                    Error = "VALIDATION_ERROR",
                    Message = message
                });
        }

        private static ObjectResult CreateStorageError()
        {
            return new ObjectResult(
                new ErrorResponse
                {
                    Error = "DOCUMENT_STORAGE_ERROR",
                    Message =
                        "The document could not be stored at this time."
                })
            {
                StatusCode =
                    StatusCodes.Status500InternalServerError
            };
        }
    }
}