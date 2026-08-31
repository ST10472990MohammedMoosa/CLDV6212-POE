// Kaden Jason Remley
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Functions/Menu/GetMenuItemsByCategory.cs

using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions.Menu
{
    // HTTP-triggered Azure Function responsible for retrieving
    // CoffeeNChill menu items belonging to a specific category.
    //
    // Endpoint:
    //
    // GET /api/menu/category/{category}
    //
    // Example:
    //
    // GET /api/menu/category/Hot%20Drinks
    //
    // The category is stored as the Azure Table Storage PartitionKey.
    //
    // The function uses IMenuItemRepository so that Azure Table
    // Storage implementation details remain inside the repository layer.

    public class GetMenuItemsByCategory
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<GetMenuItemsByCategory> _logger;

        // Constructor dependency injection.
        //
        // IMenuItemRepository provides access to MenuItems storage.
        // ILogger records successful requests, validation failures
        // and unexpected errors.

        public GetMenuItemsByCategory(
            IMenuItemRepository menuItemRepository,
            ILogger<GetMenuItemsByCategory> logger)
        {
            _menuItemRepository = menuItemRepository;
            _logger = logger;
        }

        // ============================================================
        // GET menu items by category
        // ============================================================
        //
        // Route:
        //
        // GET /api/menu/category/{category}
        //
        // Responses:
        //
        // 200 OK
        // Category exists and menu items were retrieved.
        //
        // 400 Bad Request
        // Category is missing or invalid.
        //
        // 500 Internal Server Error
        // Unexpected application or storage failure.

        [Function("GetMenuItemsByCategory")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "menu/category/{category}")]
            HttpRequest req,
            string category)
        {
            _logger.LogInformation(
                "GetMenuItemsByCategory request received for category {Category}.",
                category);

            try
            {
                // ====================================================
                // Input normalisation
                // ====================================================

                category = category?.Trim() ?? string.Empty;

                // ====================================================
                // Category validation
                // ====================================================

                if (string.IsNullOrWhiteSpace(category))
                {
                    _logger.LogWarning(
                        "GetMenuItemsByCategory validation failed: Category is required.");

                    return new BadRequestObjectResult(
                        new ErrorResponse
                        {
                            Error = "VALIDATION_ERROR",
                            Message = "Category is required."
                        });
                }

                // ====================================================
                // Retrieve menu items by category
                // ====================================================
                //
                // The repository maps the category to the Azure
                // Table Storage PartitionKey.

                var entities =
                    await _menuItemRepository.GetByCategoryAsync(
                        category);

                // ====================================================
                // Map storage entities to API response DTOs
                // ====================================================
                //
                // Azure-specific properties such as PartitionKey,
                // RowKey, Timestamp and ETag are not exposed directly
                // to the API client.

                var response =
                    entities
                        .Select(entity =>
                            new MenuItemResponse
                            {
                                Id = entity.RowKey,
                                Category = entity.PartitionKey,
                                Name = entity.Name,
                                Description = entity.Description,
                                Price = entity.Price,
                                IsAvailable = entity.IsAvailable
                            })
                        .ToList();

                _logger.LogInformation(
                    "GetMenuItemsByCategory returned {Count} menu item(s) for category {Category}.",
                    response.Count,
                    category);

                // ====================================================
                // Return HTTP 200 OK
                // ====================================================
                //
                // An empty collection is still a successful response.
                // Therefore, no 404 is returned when the category
                // contains no menu items.

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                // ====================================================
                // Unexpected error handling
                // ====================================================

                _logger.LogError(
                    ex,
                    "Unexpected error occurred while retrieving menu items for category {Category}.",
                    category);

                return new ObjectResult(
                    new ErrorResponse
                    {
                        Error = "INTERNAL_SERVER_ERROR",
                        Message =
                            "An unexpected error occurred while retrieving menu items for the specified category."
                    })
                {
                    StatusCode =
                        StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}