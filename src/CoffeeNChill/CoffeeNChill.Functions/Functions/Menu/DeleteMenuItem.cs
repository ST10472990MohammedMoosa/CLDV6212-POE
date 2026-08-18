// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Functions/Menu/DeleteMenuItem.cs

using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions.Menu
{
    // HTTP-triggered Azure Function responsible for deleting
    // an existing CoffeeNChill menu item.
    //
    // ============================================================
    // Development progression
    // ============================================================
    //
    // Commit 10:
    // - Adds DELETE /api/menu/{category}/{id}
    // - Validates Category and ID / SKU route values
    // - Checks whether the requested menu item exists
    // - Deletes the item through IMenuItemRepository
    // - Returns 200 OK after successful deletion
    // - Returns 404 Not Found when the item does not exist
    // - Adds structured error handling and logging
    //
    // CoffeeNChill Azure Table mapping:
    //
    // Category -> PartitionKey
    // ID / SKU -> RowKey
    //
    // The PartitionKey + RowKey combination uniquely identifies
    // the entity that must be deleted from the MenuItems table.
    //
    // References:
    // Microsoft Learn (2026) Azure Functions HTTP trigger.
    // Microsoft Learn (2026) Azure Table Storage entities.
    // Microsoft Learn (2026) TableClient.DeleteEntityAsync.
    public class DeleteMenuItem
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<DeleteMenuItem> _logger;

        // ============================================================
        // Commit 10 - Constructor dependency injection
        // ============================================================
        //
        // IMenuItemRepository provides the storage abstraction used
        // to retrieve and delete MenuItems.
        //
        // ILogger records successful operations, missing resources
        // and unexpected failures.
        public DeleteMenuItem(
            IMenuItemRepository menuItemRepository,
            ILogger<DeleteMenuItem> logger)
        {
            _menuItemRepository = menuItemRepository;
            _logger = logger;
        }

        // ============================================================
        // Commit 10 - Delete menu item endpoint
        // ============================================================
        //
        // Route:
        //
        // DELETE /api/menu/{category}/{id}
        //
        // Example:
        //
        // DELETE /api/menu/Hot%20Drinks/COF-001
        //
        // Responses:
        //
        // 200 OK
        // Menu item was successfully deleted.
        //
        // 400 Bad Request
        // Category or ID / SKU is invalid.
        //
        // 404 Not Found
        // Requested menu item does not exist.
        //
        // 500 Internal Server Error
        // Unexpected application or Azure Storage failure.
        [Function("DeleteMenuItem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "delete",
                Route = "menu/{category}/{id}")]
            HttpRequest req,
            string category,
            string id)
        {
            _logger.LogInformation(
                "DeleteMenuItem request received for category {Category} and ID {MenuItemId}.",
                category,
                id);

            try
            {
                // ====================================================
                // Commit 10 - Route input normalisation
                // ====================================================
                //
                // Remove leading/trailing whitespace before validating
                // or using the route values as Azure Table keys.
                category = category?.Trim() ?? string.Empty;
                id = id?.Trim() ?? string.Empty;

                // ====================================================
                // Commit 10 - Category validation
                // ====================================================
                //
                // Category is required because it represents the
                // Azure Table Storage PartitionKey.
                if (string.IsNullOrWhiteSpace(category))
                {
                    _logger.LogWarning(
                        "DeleteMenuItem validation failed: Category is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Category is required."
                        });
                }

                // ====================================================
                // Commit 10 - ID / SKU validation
                // ====================================================
                //
                // ID / SKU is required because it represents the
                // Azure Table Storage RowKey.
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning(
                        "DeleteMenuItem validation failed: ID / SKU is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Menu item ID / SKU is required."
                        });
                }

                // ====================================================
                // Commit 10 - Retrieve existing menu item
                // ====================================================
                //
                // Confirm that the entity exists before attempting
                // deletion.
                //
                // This allows the API to return a meaningful
                // 404 Not Found response instead of exposing an
                // Azure Storage exception to the client.
                var existingEntity =
                    await _menuItemRepository.GetByIdAsync(
                        category,
                        id);

                // ====================================================
                // Commit 10 - Handle missing menu item
                // ====================================================
                if (existingEntity == null)
                {
                    _logger.LogWarning(
                        "DeleteMenuItem failed: menu item {MenuItemId} was not found in category {Category}.",
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
                // Commit 10 - Delete from Azure Table Storage
                // ====================================================
                //
                // DeleteAsync uses:
                //
                // category -> PartitionKey
                // id       -> RowKey
                //
                // Storage-specific logic remains inside
                // MenuItemRepository rather than the HTTP Function.
                await _menuItemRepository.DeleteAsync(
                    category,
                    id);

                _logger.LogInformation(
                    "Menu item {MenuItemId} deleted successfully from category {Category}.",
                    id,
                    category);

                // ====================================================
                // Commit 10 - Return deletion confirmation
                // ====================================================
                //
                // Return useful information about the deleted resource
                // so the operation can be clearly verified in Postman.
                return new OkObjectResult(
                    new
                    {
                        message = "Menu item deleted successfully.",
                        deletedItem = new MenuItemResponse
                        {
                            Id = existingEntity.RowKey,
                            Category = existingEntity.PartitionKey,
                            Name = existingEntity.Name,
                            Description = existingEntity.Description,
                            Price = existingEntity.Price,
                            IsAvailable = existingEntity.IsAvailable
                        }
                    });
            }
            catch (Exception ex)
            {
                // ====================================================
                // Commit 10 - Unexpected error handling
                // ====================================================
                //
                // Technical exception details are logged internally.
                // API clients receive a controlled error response.
                _logger.LogError(
                    ex,
                    "Unexpected error occurred while deleting menu item {MenuItemId} from category {Category}.",
                    id,
                    category);

                return new ObjectResult(
                    new ErrorResponse
                    {
                        Error = "INTERNAL_SERVER_ERROR",
                        Message =
                            "An unexpected error occurred while deleting the menu item."
                    })
                {
                    StatusCode =
                        StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}