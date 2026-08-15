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
    // Reference:
    // Microsoft Learn (2026) Azure Functions HTTP trigger.
    // Microsoft Learn (2026) Dependency injection in .NET isolated worker.
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
        // Commit 4 only establishes:
        // - HTTP trigger
        // - route
        // - dependency injection
        // - logging
        // - JSON request deserialization
        //
        // Validation, duplicate detection and final Azure Table
        // persistence behaviour will be added in later commits.
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
                // Read the incoming JSON request body.
                //
                // PropertyNameCaseInsensitive allows JSON such as
                // "name" to map to the C# property "Name".
                //
                // Reference:
                // Microsoft Learn (2026) System.Text.Json deserialization.
                var request = await JsonSerializer.DeserializeAsync<CreateMenuItemRequest>(
                    req.Body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // Commit 4 intentionally stops after successful
                // deserialization.
                //
                // Validation and storage logic will be implemented
                // in subsequent commits.
                _logger.LogInformation(
                    "CreateMenuItem request body deserialized successfully.");

                return new OkObjectResult(new
                {
                    message = "CreateMenuItem endpoint is configured.",
                    request
                });
            }
            catch (JsonException ex)
            {
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