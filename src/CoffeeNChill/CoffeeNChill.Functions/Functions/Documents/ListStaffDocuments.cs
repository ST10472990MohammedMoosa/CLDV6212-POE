// Arren Naicker
// CLDV6212 Cloud Development B
// POE Part 1 - List Staff Documents
// Functions/Documents/ListStaffDocuments.cs
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
    /// Lists operational documents stored in the
    /// CoffeeNChill staff-docs Azure File Share.
    public class ListStaffDocuments
    {
        private readonly IStaffDocumentRepository _repository;
        private readonly ILogger<ListStaffDocuments> _logger;

        public ListStaffDocuments(
            IStaffDocumentRepository repository,
            ILogger<ListStaffDocuments> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [Function("ListStaffDocuments")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "documents")]
            HttpRequest req)
        {
            _logger.LogInformation(
                "Staff document list request received.");

            try
            {
                IReadOnlyList<StaffDocumentResponse> documents =
                    await _repository.GetAllAsync(
                        req.HttpContext.RequestAborted);

                _logger.LogInformation(
                    "Returning {DocumentCount} staff documents.",
                    documents.Count);

                // An empty File Share returns an empty JSON array
                // with 200 OK rather than an error.
                return new OkObjectResult(documents);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(
                    ex,
                    "Azure Files rejected the document list request. Status: {Status}.",
                    ex.Status);

                return CreateStorageError();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unexpected error occurred while listing staff documents.");

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
                        "Staff documents could not be retrieved at this time."
                })
            {
                StatusCode =
                    StatusCodes.Status500InternalServerError
            };
        }
    }
}