// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Models/MenuItemEntity.cs

using System;
using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models
{
    // Represents a single CoffeeNChill menu item stored in Azure Table Storage.
    //
    // The class implements ITableEntity so that it can be stored and retrieved
    // using the Azure.Data.Tables client library.
    //
    // Azure Table Storage uses a PartitionKey and RowKey combination to uniquely
    // identify each entity in a table.
    //
    // Reference:
    // Microsoft Learn (n.d.) ITableEntity Interface - Azure.Data.Tables.
    public class MenuItemEntity : ITableEntity
    {
        // Groups related menu items into the same Azure Table partition.
        //
        // For CoffeeNChill, the menu category is used as the PartitionKey.
        // Examples:
        // - Hot Drinks
        // - Cold Drinks
        // - Pastries
        // - Sandwiches
        //
        // This design also supports the required endpoint:
        // GET /api/menu/category/{category}
        //
        // Reference:
        // Microsoft Learn (n.d.) Understanding the Table Service Data Model.
        public string PartitionKey { get; set; } = string.Empty;


        // Uniquely identifies a menu item inside its partition.
        //
        // For CoffeeNChill, RowKey stores the unique menu item SKU / ID.
        //
        // Example:
        // PartitionKey = "Hot Drinks"
        // RowKey       = "COF-001"
        //
        // The combination of PartitionKey and RowKey uniquely identifies
        // the menu item in the MenuItems table.
        //
        // Reference:
        // Microsoft Learn (n.d.) ITableEntity Interface - Azure.Data.Tables.
        public string RowKey { get; set; } = string.Empty;


        // Stores the display name of the menu item.
        //
        // Example:
        // "Espresso"
        public string Name { get; set; } = string.Empty;


        // Stores additional information describing the menu item.
        //
        // Example:
        // "Single-shot espresso"
        public string Description { get; set; } = string.Empty;


        // Stores the selling price of the menu item.
        //
        // Azure Table Storage supports Edm.Double as a numeric property type,
        // therefore double is used for the persisted Price property.
        //
        // Application validation will later ensure that menu prices
        // cannot contain invalid or negative values.
        //
        // Reference:
        // Microsoft Learn (2023) Payload Format for Table Service Operations.
        public double Price { get; set; }


        // Indicates whether the menu item is currently available for sale.
        //
        // true  = available
        // false = unavailable
        //
        // This property is also used by the required menu update functionality.
        public bool IsAvailable { get; set; }


        // Stores the timestamp associated with the Azure Table entity.
        //
        // Azure Table Storage maintains this value on the server to indicate
        // when an entity was last modified.
        //
        // Application code should not manually manage this value during
        // normal insert or update operations.
        //
        // Reference:
        // Microsoft Learn (n.d.) TableEntity Class - Azure.Data.Tables.
        public DateTimeOffset? Timestamp { get; set; }


        // Represents the entity's ETag value.
        //
        // ETags are associated with the stored entity and can later assist
        // with concurrency-aware update operations.
        //
        // This property is required by the ITableEntity interface.
        //
        // Reference:
        // Microsoft Learn (n.d.) ITableEntity Interface - Azure.Data.Tables.
        public ETag ETag { get; set; }
    }
}