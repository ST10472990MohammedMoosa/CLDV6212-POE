// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Functions/Menu/CreateMenuItem.cs

using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoffeeNChill.Functions.Functions.Menu
{
    // HTTP-triggered Azure Function responsible for receiving
    // requests to create new CoffeeNChill menu items.
    //
    // The function depends on IMenuItemRepository instead of
    // directly communicating with Azure Table Storage.
    // This keeps the HTTP/API layer separate from storage concerns.
    //
    // Commit 5 adds input validation to the endpoint.
    // Duplicate checking and Azure Table persistence will be
    // implemented in a later commit.
    //
    // References:
    // Microsoft Learn (2026) Azure Functions HTTP trigger.
    // Microsoft Learn (2026) Dependency injection in .NET isolated worker.
    // Microsoft Learn (2026) System.Text.Json deserialization.
    public class CreateMenuItem
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<CreateMenuItem> _logger;

        // Constructor injection is used to obtain the repository
        // and logger configured through Program.cs.
        public CreateMenuItem(
            IMenuItemRepository menuItemRepository,
            ILogger<CreateMenuItem> logger)
        {
            _menuItemRepository = menuItemRepository;
            _logger = logger;
        }

        // Creates a new CoffeeNChill menu item.
        //
        // Route:
        // POST /api/menu
        //
        // Commit 5 responsibilities:
        // - HTTP trigger
        // - dependency injection
        // - logging
        // - JSON deserialization
        // - request body validation
        // - required field validation
        // - price validation
        // - basic length validation
        // - structured 400 Bad Request responses
        //
        // Duplicate detection and Azure Table persistence
        // will be added in a later commit.
        [Function("CreateMenuItem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "post",
                Route = "menu")]
            HttpRequest req)
        {
            _logger.LogInformation(
                "CreateMenuItem request received.");

            try
            {
                // Deserialize the incoming JSON request body.
                //
                // PropertyNameCaseInsensitive allows JSON properties such as
                // "name" to map correctly to the C# property "Name".
                var request = await JsonSerializer.DeserializeAsync<CreateMenuItemRequest>(
                    req.Body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // A null request indicates that no valid request body
                // could be deserialized.
                if (request == null)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: request body is required.");

                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = "A request body is required."
                    });
                }

                // Normalise string values before performing validation.
                //
                // Trimming prevents values containing only spaces from
                // passing required-field checks.
                request.Id = request.Id?.Trim() ?? string.Empty;
                request.Category = request.Category?.Trim() ?? string.Empty;
                request.Name = request.Name?.Trim() ?? string.Empty;
                request.Description = request.Description?.Trim() ?? string.Empty;

                // Validate menu item ID / SKU.
                //
                // The ID will later be mapped to the Azure Table Storage RowKey,
                // therefore every menu item requires a valid identifier.
                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Id is required.");

                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = "Menu item ID / SKU is required."
                    });
                }

                // Validate the menu category.
                //
                // The category will later be mapped to the Azure Table Storage
                // PartitionKey and is therefore required.
                if (string.IsNullOrWhiteSpace(request.Category))
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Category is required.");

                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = "Category is required."
                    });
                }

                // Validate the menu item name.
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Name is required.");

                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = "Menu item name is required."
                    });
                }

                // Validate the menu item price.
                //
                // A CoffeeNChill menu item must have a positive monetary value.
                if (request.Price <= 0)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Price must be greater than zero.");

                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = "Price must be greater than zero."
                    });
                }

                // Prevent excessively long menu item identifiers.
                if (request.Id.Length > 50)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Id exceeds maximum length.");

                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = "Menu item ID / SKU cannot exceed 50 characters."
                    });
                }

                // Prevent excessively long menu item category values.
                if (request.Category.Length > 100)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Category exceeds maximum length.");

                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = "Category cannot exceed 100 characters."
                    });
                }

                // Prevent excessively long menu item names.
                if (request.Name.Length > 100)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Name exceeds maximum length.");

                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = "Menu item name cannot exceed 100 characters."
                    });
                }

                // Description is optional, but if supplied it should remain
                // within a sensible API payload size.
                if (request.Description.Length > 500)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Description exceeds maximum length.");

                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "VALIDATION_ERROR",
                        Message = "Description cannot exceed 500 characters."
                    });
                }

                _logger.LogInformation(
                    "CreateMenuItem request passed validation for menu item {MenuItemId}.",
                    request.Id);

                // Commit 5 intentionally stops after successful validation.
                //
                // Commit 6 will:
                // - check whether the menu item already exists
                // - return 409 Conflict for duplicates
                // - map the DTO to MenuItemEntity
                // - persist the entity to Azure Table Storage
                // - return the final successful creation response
                return new OkObjectResult(new
                {
                    message = "CreateMenuItem request passed validation.",
                    request
                });
            }
            catch (JsonException ex)
            {
                // Invalid JSON must return 400 instead of allowing
                // an unhandled deserialization exception.
                _logger.LogWarning(
                    ex,
                    "CreateMenuItem received invalid JSON.");

                return new BadRequestObjectResult(new ErrorResponse
                {
                    Error = "INVALID_JSON",
                    Message = "The request body contains invalid JSON."
                });
            }
        }
    }
}