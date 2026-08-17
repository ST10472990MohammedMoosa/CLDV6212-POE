// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Functions/Menu/CreateMenuItem.cs

using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoffeeNChill.Functions.Functions.Menu
{
    // HTTP-triggered Azure Function responsible for creating
    // new CoffeeNChill menu items.
    //
    // The function depends on IMenuItemRepository instead of
    // directly communicating with Azure Table Storage.
    // This keeps the HTTP/API layer separate from storage concerns.
    //
    // ============================================================
    // Development progression
    // ============================================================
    //
    // Commit 4:
    // - Created the CreateMenuItem HTTP-triggered Azure Function
    // - Configured POST /api/menu
    // - Added constructor dependency injection
    // - Added request logging
    // - Added JSON request deserialization
    //
    // Commit 5:
    // - Added request body validation
    // - Added required field validation
    // - Added positive price validation
    // - Added maximum length validation
    // - Added input trimming and normalisation
    // - Added structured 400 Bad Request responses
    // - Added invalid JSON handling
    //
    // Commit 6:
    // - Adds duplicate menu item detection
    // - Adds 409 Conflict responses
    // - Maps CreateMenuItemRequest to MenuItemEntity
    // - Persists menu items through IMenuItemRepository
    // - Returns 201 Created for successful creation
    // - Adds structured 500 Internal Server Error handling
    //
    // References:
    // Microsoft Learn (2026) Azure Functions HTTP trigger.
    // Microsoft Learn (2026) Dependency injection in .NET isolated worker.
    // Microsoft Learn (2026) System.Text.Json deserialization.
    // Microsoft Learn (2026) Azure Tables client library for .NET.
    // Microsoft Learn (2026) TableClient methods and Azure Table entities.
    public class CreateMenuItem
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<CreateMenuItem> _logger;

        // ============================================================
        // Commit 4 - Constructor dependency injection
        // ============================================================
        //
        // Constructor injection is used to obtain the repository
        // and logger configured through Program.cs.
        //
        // Using IMenuItemRepository keeps this HTTP function
        // independent from the concrete Azure Table Storage service.
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
        // Responses:
        //
        // 201 Created
        // Menu item was successfully stored.
        //
        // 400 Bad Request
        // Request body or menu item data is invalid.
        //
        // 409 Conflict
        // A menu item with the same Category and ID already exists.
        //
        // 500 Internal Server Error
        // An unexpected application or storage error occurred.
        [Function("CreateMenuItem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "post",
                Route = "menu")]
            HttpRequest req)
        {
            // ========================================================
            // Commit 4 - Request logging
            // ========================================================

            _logger.LogInformation(
                "CreateMenuItem request received.");

            try
            {
                // ====================================================
                // Commit 4 - JSON request deserialization
                // ====================================================
                //
                // Deserialize the incoming JSON body into the
                // CreateMenuItemRequest DTO.
                //
                // PropertyNameCaseInsensitive allows JSON properties
                // such as "name" to map to the C# property "Name".
                //
                // Reference:
                // Microsoft Learn (2026) System.Text.Json deserialization.

                var request =
                    await JsonSerializer.DeserializeAsync<CreateMenuItemRequest>(
                        req.Body,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                // ====================================================
                // Commit 5 - Request body validation
                // ====================================================
                //
                // A null object means that no usable request body
                // could be deserialized.

                if (request == null)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: request body is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "A request body is required."
                        });
                }

                // ====================================================
                // Commit 5 - Input normalisation
                // ====================================================
                //
                // Trim incoming string values before validation.
                //
                // This prevents values containing only whitespace
                // from passing required-field validation.

                request.Id =
                    request.Id?.Trim() ?? string.Empty;

                request.Category =
                    request.Category?.Trim() ?? string.Empty;

                request.Name =
                    request.Name?.Trim() ?? string.Empty;

                request.Description =
                    request.Description?.Trim() ?? string.Empty;

                // ====================================================
                // Commit 5 - Menu item ID / SKU validation
                // ====================================================
                //
                // The ID is later mapped to the Azure Table RowKey,
                // therefore it is required.

                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Id is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Menu item ID / SKU is required."
                        });
                }

                // ====================================================
                // Commit 5 - Category validation
                // ====================================================
                //
                // Category is later mapped to the Azure Table
                // PartitionKey and is therefore required.

                if (string.IsNullOrWhiteSpace(request.Category))
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Category is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Category is required."
                        });
                }

                // ====================================================
                // Commit 5 - Menu item name validation
                // ====================================================

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Name is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Menu item name is required."
                        });
                }

                // ====================================================
                // Commit 5 - Price validation
                // ====================================================
                //
                // CoffeeNChill menu items must have a positive
                // monetary value.

                if (request.Price <= 0)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Price must be greater than zero.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Price must be greater than zero."
                        });
                }

                // ====================================================
                // Commit 5 - ID maximum length validation
                // ====================================================

                if (request.Id.Length > 50)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Id exceeds maximum length.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Menu item ID / SKU cannot exceed 50 characters."
                        });
                }

                // ====================================================
                // Commit 5 - Category maximum length validation
                // ====================================================

                if (request.Category.Length > 100)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Category exceeds maximum length.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Category cannot exceed 100 characters."
                        });
                }

                // ====================================================
                // Commit 5 - Name maximum length validation
                // ====================================================

                if (request.Name.Length > 100)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Name exceeds maximum length.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Menu item name cannot exceed 100 characters."
                        });
                }

                // ====================================================
                // Commit 5 - Description maximum length validation
                // ====================================================
                //
                // Description is optional, but if supplied,
                // it must remain within the API limit.

                if (request.Description.Length > 500)
                {
                    _logger.LogWarning(
                        "CreateMenuItem validation failed: Description exceeds maximum length.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Description cannot exceed 500 characters."
                        });
                }

                _logger.LogInformation(
                    "CreateMenuItem request passed validation for menu item {MenuItemId}.",
                    request.Id);

                // ====================================================
                // Commit 6 - Duplicate menu item detection
                // ====================================================
                //
                // Azure Table Storage uniquely identifies an entity
                // using its PartitionKey and RowKey.
                //
                // CoffeeNChill maps:
                //
                // Category -> PartitionKey
                // ID / SKU -> RowKey
                //
                // Therefore, checking Category + ID tells us whether
                // the requested menu item already exists.
                //
                // Reference:
                // Microsoft Learn (2026) Azure Table Storage entities.

                var existingMenuItem =
                    await _menuItemRepository.GetByIdAsync(
                        request.Category,
                        request.Id);

                if (existingMenuItem != null)
                {
                    _logger.LogWarning(
                        "CreateMenuItem conflict: menu item {MenuItemId} already exists in category {Category}.",
                        request.Id,
                        request.Category);

                    return new ConflictObjectResult(
                        new ErrorResponse
                        {
                            Error = "DUPLICATE_MENU_ITEM",
                            Message =
                                "A menu item with the same category and ID / SKU already exists."
                        });
                }

                // ====================================================
                // Commit 6 - DTO to Azure Table entity mapping
                // ====================================================
                //
                // Convert the validated API DTO into the storage
                // entity used by Azure.Data.Tables.
                //
                // Category becomes PartitionKey.
                // ID / SKU becomes RowKey.

                var entity =
                    new MenuItemEntity
                    {
                        PartitionKey = request.Category,
                        RowKey = request.Id,
                        Name = request.Name,
                        Description = request.Description,
                        Price = request.Price,
                        IsAvailable = request.IsAvailable
                    };

                // ====================================================
                // Commit 6 - Persist menu item
                // ====================================================
                //
                // Save through IMenuItemRepository rather than calling
                // TableClient directly from the Function.
                //
                // This preserves separation between:
                //
                // HTTP/API layer
                // and
                // Azure Table Storage layer.

                await _menuItemRepository.CreateAsync(entity);

                _logger.LogInformation(
                    "Menu item {MenuItemId} created successfully in category {Category}.",
                    request.Id,
                    request.Category);

                // ====================================================
                // Commit 6 - Build API response DTO
                // ====================================================
                //
                // Return MenuItemResponse rather than exposing the
                // Azure Table Storage entity directly to API clients.

                var response =
                    new MenuItemResponse
                    {
                        Id = entity.RowKey,
                        Category = entity.PartitionKey,
                        Name = entity.Name,
                        Description = entity.Description,
                        Price = entity.Price,
                        IsAvailable = entity.IsAvailable
                    };

                // ====================================================
                // Commit 6 - Return HTTP 201 Created
                // ====================================================
                //
                // HTTP 201 indicates that a new resource
                // was successfully created.

                return new ObjectResult(response)
                {
                    StatusCode = StatusCodes.Status201Created
                };
            }

            // ========================================================
            // Commit 5 - Invalid JSON handling
            // ========================================================
            //
            // JsonException is handled separately so malformed JSON
            // produces a clear 400 Bad Request response rather than
            // an unhandled server error.

            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    "CreateMenuItem received invalid JSON.");

                return new BadRequestObjectResult(
                    new ErrorResponse
                    {
                        Error = "INVALID_JSON",
                        Message =
                            "The request body contains invalid JSON."
                    });
            }

            // ========================================================
            // Commit 6 - Unexpected error handling
            // ========================================================
            //
            // Any unexpected repository, Azure Storage or application
            // error returns a structured 500 response.
            //
            // Detailed technical information is written to the server
            // log rather than exposed to the API client.

            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred while creating menu item.");

                return new ObjectResult(
                    new ErrorResponse
                    {
                        Error = "INTERNAL_SERVER_ERROR",
                        Message =
                            "An unexpected error occurred while creating the menu item."
                    })
                {
                    StatusCode =
                        StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}