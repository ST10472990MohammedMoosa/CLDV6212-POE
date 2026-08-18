// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// DTOs/MenuItemResponse.cs

namespace CoffeeNChill.Functions.DTOs
{
    // Defines the clean JSON representation returned to API clients
    // when CoffeeNChill menu data is requested.
    //
    // This DTO prevents Azure-specific storage properties such as
    // PartitionKey, RowKey, Timestamp and ETag from being exposed
    // directly through the public API.
    //
    // Instead:
    //
    // PartitionKey -> Category
    // RowKey       -> Id
    //
    // This creates a clearer API contract while keeping Azure Table
    // implementation details inside the storage layer.
    public class MenuItemResponse
    {
        // Public representation of the Azure Table PartitionKey.
        public string Category { get; set; } = string.Empty;


        // Public representation of the Azure Table RowKey / SKU.
        public string Id { get; set; } = string.Empty;


        // Menu item display name.
        public string Name { get; set; } = string.Empty;


        // Menu item description.
        public string Description { get; set; } = string.Empty;


        // Current selling price.
        public double Price { get; set; }


        // Current menu availability state.
        public bool IsAvailable { get; set; }
    }
}