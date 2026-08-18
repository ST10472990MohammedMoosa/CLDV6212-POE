// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// DTOs/CreateMenuItemRequest.cs

namespace CoffeeNChill.Functions.DTOs
{
    // Represents the JSON request body received when a new CoffeeNChill
    // menu item is created through:
    //
    // POST /api/menu
    //
    // This DTO separates the public API request structure from the
    // Azure Table Storage entity used internally by the application.
    //
    // The request data will later be validated and mapped to MenuItemEntity.
    //
    // Reference:
    // Microsoft Learn (n.d.) Data transfer objects and API model design.
    public class CreateMenuItemRequest
    {
        // Menu category supplied by the client.
        //
        // This value will later be mapped to MenuItemEntity.PartitionKey.
        //
        // Example:
        // "Hot Drinks"
        public string Category { get; set; } = string.Empty;


        // Unique menu item SKU / ID supplied by the client.
        //
        // This value will later be mapped to MenuItemEntity.RowKey.
        //
        // Example:
        // "COF-001"
        public string Id { get; set; } = string.Empty;


        // Human-readable menu item name.
        //
        // Example:
        // "Espresso"
        public string Name { get; set; } = string.Empty;


        // Additional description displayed for the menu item.
        //
        // Example:
        // "Single-shot espresso"
        public string Description { get; set; } = string.Empty;


        // Selling price supplied by the client.
        //
        // Validation will later reject negative or otherwise invalid prices
        // before the request is mapped to the Azure Table entity.
        public double Price { get; set; }


        // Indicates whether the menu item is currently available.
        //
        // true  = available
        // false = unavailable
        public bool IsAvailable { get; set; }
    }
}