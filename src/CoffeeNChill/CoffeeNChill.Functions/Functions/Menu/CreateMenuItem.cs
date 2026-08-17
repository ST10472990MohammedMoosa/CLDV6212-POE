// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Functions/Menu/GetMenuItems.cs

using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions.Menu
{
    // HTTP-triggered Azure Function responsible for retrieving
    // CoffeeNChill menu items from Azure Table Storage.
    //
    // ============================================================
    // Development progression
    // ============================================================
    //
    // Commit 7:
    // - Adds GET /api/menu
    // - Retrieves all menu items
    // - Supports optional category filtering
    // - Maps storage entities to MenuItemResponse DTOs
    // - Returns 200 OK responses
    // - Adds structured error handling and logging
    //
    // The function uses IMenuItemRepository rather than accessing
    // Azure Table Storage directly.
    //
    // References:
    // Microsoft Learn (2026) Azure Functions HTTP trigger.
    // Microsoft Learn (2026) Dependency injection in .NET isolated worker.
    // Microsoft Learn (2026) Azure Tables client library for .NET.
    public class GetMenuItems
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<GetMenuItems> _logger;

        // ============================================================
        // Commit 7 - Constructor dependency injection
        // ============================================================
        //
        // The repository provides access to menu item storage.
        // The logger records function execution and errors.
        public GetMenuItems(
            IMenuItemRepository menuItemRepository,
            ILogger<GetMenuItems> logger)
        {
            _menuItemRepository = menuItemRepository;
            _logger = logger;
        }

        // ============================================================
        // Commit 7 - GET menu items endpoint
        // ============================================================
        //
        // Routes:
        //
        // GET /api/menu
        // Returns all menu items.
        //
        // GET /api/menu?category=Hot Drinks
        // Returns only menu items from the supplied category.
        //
        // Responses:
        //
        // 200 OK
        // Menu items retrieved successfully.
        //
        // 500 Internal Server Error
        // An unexpected application or storage error occurred.
        [Function("GetMenuItems")]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "menu")]
            HttpRequest req)
        {
            _logger.LogInformation(
                "GetMenuItems request received.");

            try
            {
                // ====================================================
                // Commit 7 - Read optional category query parameter
                // ====================================================
                //
                // A category is optional.
                //
                // If supplied, the repository retrieves only entities
                // from that Azure Table Storage PartitionKey.
                //
                // If omitted, all menu items are returned.
                var category =
                    req.Query["category"].FirstOrDefault()?.Trim();

                IEnumerable<Models.MenuItemEntity> entities;

                if (!string.IsNullOrWhiteSpace(category))
                {
                    _logger.LogInformation(
                        "Retrieving menu items for category {Category}.",
                        category);

                    // =================================================
                    // Commit 7 - Retrieve by category
                    // =================================================
                    //
                    // Category maps to the Azure Table PartitionKey.
                    entities =
                        await _menuItemRepository.GetByCategoryAsync(
                            category);
                }
                else
                {
                    _logger.LogInformation(
                        "Retrieving all CoffeeNChill menu items.");

                    // =================================================
                    // Commit 7 - Retrieve all menu items
                    // =================================================
                    entities =
                        await _menuItemRepository.GetAllAsync();
                }

                // ====================================================
                // Commit 7 - Map entities to API response DTOs
                // ====================================================
                //
                // The Azure Table Storage entities are converted into
                // MenuItemResponse objects so storage-specific fields
                // are not exposed directly through the API.
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
                    "GetMenuItems returned {Count} menu item(s).",
                    response.Count);

                // ====================================================
                // Commit 7 - Return HTTP 200 OK
                // ====================================================
                //
                // An empty collection is still a successful response.
                // Therefore no 404 is returned when zero items exist.
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                // ====================================================
                // Commit 7 - Unexpected error handling
                // ====================================================
                _logger.LogError(
                    ex,
                    "Unexpected error occurred while retrieving menu items.");

                return new ObjectResult(
                    new ErrorResponse
                    {
                        Error = "INTERNAL_SERVER_ERROR",
                        Message =
                            "An unexpected error occurred while retrieving menu items."
                    })
                {
                    StatusCode =
                        StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}