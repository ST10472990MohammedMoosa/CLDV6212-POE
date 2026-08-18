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
    // ============================================================
    // Development progression
    // ============================================================
    //
    // Commit 4:
    // - Created CreateMenuItem HTTP function
    // - Added POST /api/menu
    // - Added dependency injection
    // - Added JSON deserialization
    //
    // Commit 5:
    // - Added input validation
    // - Added required field validation
    // - Added positive price validation
    // - Added length validation
    // - Added structured 400 responses
    //
    // Commit 6:
    // - Added duplicate checking
    // - Added Azure Table persistence
    // - Added 409 Conflict handling
    // - Added 201 Created response
    // - Added structured 500 error handling
    //
    // References:
    // Microsoft Learn (2026) Azure Functions HTTP trigger.
    // Microsoft Learn (2026) Dependency injection in .NET isolated worker.
    // Microsoft Learn (2026) System.Text.Json deserialization.
    // Microsoft Learn (2026) Azure Tables client library for .NET.
    public class CreateMenuItem
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<CreateMenuItem> _logger;

        public CreateMenuItem(
            IMenuItemRepository menuItemRepository,
            ILogger<CreateMenuItem> logger)
        {
            _menuItemRepository = menuItemRepository;
            _logger = logger;
        }

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
                // ====================================================
                // Commit 4 - JSON deserialization
                // ====================================================
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
                if (request == null)
                {
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
                request.Id =
                    request.Id?.Trim() ?? string.Empty;

                request.Category =
                    request.Category?.Trim() ?? string.Empty;

                request.Name =
                    request.Name?.Trim() ?? string.Empty;

                request.Description =
                    request.Description?.Trim() ?? string.Empty;

                // ====================================================
                // Commit 5 - ID / SKU validation
                // ====================================================
                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Menu item ID / SKU is required."
                        });
                }

                // ====================================================
                // Commit 5 - Category validation
                // ====================================================
                if (string.IsNullOrWhiteSpace(request.Category))
                {
                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Category is required."
                        });
                }

                // ====================================================
                // Commit 5 - Name validation
                // ====================================================
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Menu item name is required."
                        });
                }

                // ====================================================
                // Commit 5 - Price validation
                // ====================================================
                if (request.Price <= 0)
                {
                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Price must be greater than zero."
                        });
                }

                // ====================================================
                // Commit 5 - Maximum length validation
                // ====================================================
                if (request.Id.Length > 50)
                {
                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Menu item ID / SKU cannot exceed 50 characters."
                        });
                }

                if (request.Category.Length > 100)
                {
                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Category cannot exceed 100 characters."
                        });
                }

                if (request.Name.Length > 100)
                {
                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Menu item name cannot exceed 100 characters."
                        });
                }

                if (request.Description.Length > 500)
                {
                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "Description cannot exceed 500 characters."
                        });
                }

                // ====================================================
                // Commit 6 - Duplicate menu item detection
                // ====================================================
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
                // Commit 6 - DTO to entity mapping
                // ====================================================
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
                await _menuItemRepository.CreateAsync(entity);

                _logger.LogInformation(
                    "Menu item {MenuItemId} created successfully in category {Category}.",
                    request.Id,
                    request.Category);

                // ====================================================
                // Commit 6 - Build API response
                // ====================================================
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
                // Commit 6 - Return 201 Created
                // ====================================================
                return new ObjectResult(response)
                {
                    StatusCode =
                        StatusCodes.Status201Created
                };
            }
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