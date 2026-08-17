// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Functions/Menu/GetMenuItemById.cs

using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions.Menu
{
    // HTTP-triggered Azure Function responsible for retrieving
    // one CoffeeNChill menu item by Category and ID / SKU.
    //
    // ============================================================
    // Development progression
    // ============================================================
    //
    // Commit 8:
    // - Adds GET /api/menu/{category}/{id}
    // - Retrieves one menu item using PartitionKey + RowKey
    // - Maps MenuItemEntity to MenuItemResponse
    // - Returns 200 OK when the menu item exists
    // - Returns 404 Not Found when the menu item does not exist
    // - Adds validation, structured error responses and logging
    //
    // CoffeeNChill Azure Table mapping:
    //
    // Category -> PartitionKey
    // ID / SKU -> RowKey
    //
    // References:
    // Microsoft Learn (2026) Azure Functions HTTP trigger.
    // Microsoft Learn (2026) Azure Table Storage entities.
    // Microsoft Learn (2026) TableClient.GetEntityAsync method.
    public class GetMenuItemById
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<GetMenuItemById> _logger;

        // ============================================================
        // Commit 8 - Constructor dependency injection
        // ============================================================
        public GetMenuItemById(
            IMenuItemRepository menuItemRepository,
            ILogger<GetMenuItemById> logger)
        {
            _menuItemRepository = menuItemRepository;
            _logger = logger;
        }

        // ============================================================
        // Commit 8 - GET single menu item endpoint
        // ============================================================
        //
        // Route:
        //
        // GET /api/menu/{category}/{id}
        //
        // Example:
        //
        // GET /api/menu/Hot%20Drinks/COF-001
        //
        // Responses:
        //
        // 200 OK
        // Requested menu item exists.
        //
        // 400 Bad Request
        // Category or ID is missing/invalid.
        //
        // 404 Not Found
        // Requested menu item does not exist.
        //
        // 500 Internal Server Error
        // Unexpected application or storage failure.
        [Function("GetMenuItemById")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "menu/{category}/{id}")]
            HttpRequest req,
            string category,
            string id)
        {
            _logger.LogInformation(
                "GetMenuItemById request received for category {Category} and ID {MenuItemId}.",
                category,
                id);

            try
            {
                // ====================================================
                // Commit 8 - Input normalisation
                // ====================================================
                category = category?.Trim() ?? string.Empty;
                id = id?.Trim() ?? string.Empty;

                // ====================================================
                // Commit 8 - Category validation
                // ====================================================
                if (string.IsNullOrWhiteSpace(category))
                {
                    _logger.LogWarning(
                        "GetMenuItemById validation failed: Category is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Category is required."
                        });
                }

                // ====================================================
                // Commit 8 - ID / SKU validation
                // ====================================================
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning(
                        "GetMenuItemById validation failed: ID / SKU is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Menu item ID / SKU is required."
                        });
                }

                // ====================================================
                // Commit 8 - Retrieve entity using PartitionKey + RowKey
                // ====================================================
                //
                // The repository translates this request into an
                // Azure Table lookup using:
                //
                // PartitionKey = category
                // RowKey       = id
                var entity =
                    await _menuItemRepository.GetByIdAsync(
                        category,
                        id);

                // ====================================================
                // Commit 8 - Handle missing menu item
                // ====================================================
                if (entity == null)
                {
                    _logger.LogWarning(
                        "Menu item {MenuItemId} was not found in category {Category}.",
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
                // Commit 8 - Map entity to API response DTO
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

                _logger.LogInformation(
                    "Menu item {MenuItemId} retrieved successfully from category {Category}.",
                    id,
                    category);

                // ====================================================
                // Commit 8 - Return HTTP 200 OK
                // ====================================================
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                // ====================================================
                // Commit 8 - Unexpected error handling
                // ====================================================
                _logger.LogError(
                    ex,
                    "Unexpected error occurred while retrieving menu item {MenuItemId} from category {Category}.",
                    id,
                    category);

                return new ObjectResult(
                    new ErrorResponse
                    {
                        Error = "INTERNAL_SERVER_ERROR",
                        Message =
                            "An unexpected error occurred while retrieving the menu item."
                    })
                {
                    StatusCode =
                        StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}