// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - Download Staff Document
// Functions/Documents/DownloadStaffDocument.cs
//
// Microsoft Learn (n.d.) ShareFileClient Class.
// Microsoft Learn (n.d.) Azure Functions HTTP trigger.
// Microsoft Learn (n.d.) FileStreamResult Class.
// Microsoft Learn (n.d.) Azure Functions .NET isolated worker guide.

using Azure;
using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions.Documents
{
    /// Downloads and streams a PDF from the
    /// CoffeeNChill staff-docs Azure File Share.
    public class DownloadStaffDocument
    {
        private readonly IStaffDocumentRepository _repository;
        private readonly ILogger<DownloadStaffDocument> _logger;

        public DownloadStaffDocument(
            IStaffDocumentRepository repository,
            ILogger<DownloadStaffDocument> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [Function("DownloadStaffDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "documents/download/{fileName}")]
            HttpRequest req,
            string fileName)
        {
            fileName = fileName?.Trim() ?? string.Empty;

            string? validationError =
                StaffDocumentValidator.ValidateFileName(fileName);

            if (validationError != null)
            {
                _logger.LogWarning(
                    "Document download validation failed: {ValidationError}",
                    validationError);

                return new BadRequestObjectResult(
                    new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = validationError
                    });
            }

            try
            {
                Stream? content =
                    await _repository.DownloadAsync(
                        fileName,
                        req.HttpContext.RequestAborted);

                if (content == null)
                {
                    _logger.LogWarning(
                        "Staff document {FileName} was not found.",
                        fileName);

                    return new NotFoundObjectResult(
                        new ErrorResponse
                        {
                            Error = "DOCUMENT_NOT_FOUND",
                            Message =
                                "The requested staff document could not be found."
                        });
                }

                _logger.LogInformation(
                    "Streaming staff document {FileName}.",
                    fileName);

                return new FileStreamResult(
                    content,
                    "application/pdf")
                {
                    FileDownloadName = fileName,
                    EnableRangeProcessing = true
                };
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(
                    ex,
                    "Azure Files rejected the document download. Status: {Status}.",
                    ex.Status);

                return CreateStorageError();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unexpected error occurred while downloading {FileName}.",
                    fileName);

                return CreateStorageError();
            }
        }

        private static ObjectResult CreateStorageError()
        {
            return new ObjectResult(
                new ErrorResponse
                {
                    Error = "DOCUMENT_STORAGE_ERROR",
                    Message =
                        "The document could not be downloaded at this time."
                })
            {
                StatusCode =
                    StatusCodes.Status500InternalServerError
            };
        }
    }
}