// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Functions/Menu/UpdateMenuItem.cs

using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoffeeNChill.Functions.Functions.Menu
{
    // HTTP-triggered Azure Function responsible for updating
    // an existing CoffeeNChill menu item.
    //
    // ============================================================
    // Development progression
    // ============================================================
    //
    // Commit 9:
    // - Adds PUT /api/menu/{category}/{id}
    // - Retrieves the existing menu item
    // - Supports partial updates
    // - Updates Price when supplied
    // - Updates IsAvailable when supplied
    // - Persists changes through IMenuItemRepository
    // - Returns 200 OK after successful update
    // - Returns 400 Bad Request for invalid update requests
    // - Returns 404 Not Found if the item does not exist
    // - Adds structured 500 Internal Server Error handling
    //
    // CoffeeNChill Azure Table mapping:
    //
    // Category -> PartitionKey
    // ID / SKU -> RowKey
    //
    // References:
    // Microsoft Learn (2026) Azure Functions HTTP trigger.
    // Microsoft Learn (2026) Azure Table Storage entities.
    // Microsoft Learn (2026) System.Text.Json deserialization.
    public class UpdateMenuItem
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<UpdateMenuItem> _logger;

        // ============================================================
        // Commit 9 - Constructor dependency injection
        // ============================================================
        public UpdateMenuItem(
            IMenuItemRepository menuItemRepository,
            ILogger<UpdateMenuItem> logger)
        {
            _menuItemRepository = menuItemRepository;
            _logger = logger;
        }

        // ============================================================
        // Commit 9 - Update menu item endpoint
        // ============================================================
        //
        // Route:
        //
        // PUT /api/menu/{category}/{id}
        //
        // Example:
        //
        // PUT /api/menu/Hot%20Drinks/COF-001
        //
        // The request body supports partial updates:
        //
        // {
        //   "price": 32.00
        // }
        //
        // or:
        //
        // {
        //   "isAvailable": false
        // }
        //
        // or both:
        //
        // {
        //   "price": 32.00,
        //   "isAvailable": false
        // }
        //
        // Responses:
        //
        // 200 OK
        // Menu item updated successfully.
        //
        // 400 Bad Request
        // Route values or request body are invalid.
        //
        // 404 Not Found
        // Menu item does not exist.
        //
        // 500 Internal Server Error
        // Unexpected application or storage error.
        [Function("UpdateMenuItem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "put",
                Route = "menu/{category}/{id}")]
            HttpRequest req,
            string category,
            string id)
        {
            _logger.LogInformation(
                "UpdateMenuItem request received for category {Category} and ID {MenuItemId}.",
                category,
                id);

            try
            {
                // ====================================================
                // Commit 9 - Route input normalisation
                // ====================================================
                category = category?.Trim() ?? string.Empty;
                id = id?.Trim() ?? string.Empty;

                // ====================================================
                // Commit 9 - Category validation
                // ====================================================
                if (string.IsNullOrWhiteSpace(category))
                {
                    _logger.LogWarning(
                        "UpdateMenuItem validation failed: Category is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Category is required."
                        });
                }

                // ====================================================
                // Commit 9 - ID / SKU validation
                // ====================================================
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning(
                        "UpdateMenuItem validation failed: ID / SKU is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Menu item ID / SKU is required."
                        });
                }

                // ====================================================
                // Commit 9 - Deserialize partial update request
                // ====================================================
                //
                // UpdateMenuItemRequest deliberately uses nullable
                // properties:
                //
                // double? Price
                // bool? IsAvailable
                //
                // null means the client did not request a change
                // to that property.
                var request =
                    await JsonSerializer.DeserializeAsync<UpdateMenuItemRequest>(
                        req.Body,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (request == null)
                {
                    _logger.LogWarning(
                        "UpdateMenuItem validation failed: request body is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "A request body is required."
                        });
                }

                // ====================================================
                // Commit 9 - Require at least one update field
                // ====================================================
                //
                // A request containing neither Price nor IsAvailable
                // does not contain anything to update.
                if (!request.Price.HasValue &&
                    !request.IsAvailable.HasValue)
                {
                    _logger.LogWarning(
                        "UpdateMenuItem validation failed: no update values were supplied.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message =
                                "At least one of Price or IsAvailable must be supplied."
                        });
                }

                // ====================================================
                // Commit 9 - Price validation
                // ====================================================
                //
                // Price is optional, but if it is supplied,
                // it must be greater than zero.
                if (request.Price.HasValue &&
                    request.Price.Value <= 0)
                {
                    _logger.LogWarning(
                        "UpdateMenuItem validation failed: Price must be greater than zero.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Price must be greater than zero."
                        });
                }

                // ====================================================
                // Commit 9 - Retrieve existing menu item
                // ====================================================
                //
                // Category maps to PartitionKey.
                // ID / SKU maps to RowKey.
                var existingEntity =
                    await _menuItemRepository.GetByIdAsync(
                        category,
                        id);

                if (existingEntity == null)
                {
                    _logger.LogWarning(
                        "UpdateMenuItem failed: menu item {MenuItemId} was not found in category {Category}.",
                        id,
                        category);

                    return new NotFoundObjectResult(
                        new ErrorResponse
                        {
                            Error = "MENU_ITEM_NOT_FOUND",
                            Message =
                                "The requested menu item could not be found."
                        });
                }

                // ====================================================
                // Commit 9 - Apply partial Price update
                // ====================================================
                //
                // Only overwrite Price when the client supplied
                // a replacement value.
                if (request.Price.HasValue)
                {
                    existingEntity.Price =
                        request.Price.Value;
                }

                // ====================================================
                // Commit 9 - Apply partial availability update
                // ====================================================
                //
                // bool? allows false to be distinguished from null.
                //
                // null  = leave current value unchanged
                // true  = mark item available
                // false = mark item unavailable
                if (request.IsAvailable.HasValue)
                {
                    existingEntity.IsAvailable =
                        request.IsAvailable.Value;
                }

                // ====================================================
                // Commit 9 - Persist updated entity
                // ====================================================
                await _menuItemRepository.UpdateAsync(existingEntity);

                _logger.LogInformation(
                    "Menu item {MenuItemId} updated successfully in category {Category}.",
                    id,
                    category);

                // ====================================================
                // Commit 9 - Build API response DTO
                // ====================================================
                var response =
                    new MenuItemResponse
                    {
                        Id = existingEntity.RowKey,
                        Category = existingEntity.PartitionKey,
                        Name = existingEntity.Name,
                        Description = existingEntity.Description,
                        Price = existingEntity.Price,
                        IsAvailable = existingEntity.IsAvailable
                    };

                // ====================================================
                // Commit 9 - Return HTTP 200 OK
                // ====================================================
                return new OkObjectResult(response);
            }
            catch (JsonException ex)
            {
                // ====================================================
                // Commit 9 - Invalid JSON handling
                // ====================================================
                _logger.LogWarning(
                    ex,
                    "UpdateMenuItem received invalid JSON.");

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
                // ====================================================
                // Commit 9 - Unexpected error handling
                // ====================================================
                _logger.LogError(
                    ex,
                    "Unexpected error occurred while updating menu item {MenuItemId} in category {Category}.",
                    id,
                    category);

                return new ObjectResult(
                    new ErrorResponse
                    {
                        Error = "INTERNAL_SERVER_ERROR",
                        Message =
                            "An unexpected error occurred while updating the menu item."
                    })
                {
                    StatusCode =
                        StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}