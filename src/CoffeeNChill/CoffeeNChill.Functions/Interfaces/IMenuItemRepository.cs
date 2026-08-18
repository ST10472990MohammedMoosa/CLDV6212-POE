/// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Interfaces/IMenuItemRepository.cs

using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Interfaces
{
    // Defines the storage operations that the CoffeeNChill application
    // requires for menu items.
    //
    // The interface separates the Azure Table Storage implementation
    // from the HTTP Function layer.
    //
    // This means HTTP Functions such as CreateMenuItem and GetAllMenuItems
    // do not need to know how Azure Table Storage is accessed internally.
    //
    // Instead:
    //
    // HTTP Function
    //      |
    //      v
    // IMenuItemRepository
    //      |
    //      v
    // MenuItemRepository
    //      |
    //      v
    // Azure.Data.Tables TableClient
    //
    // This separation makes the application easier to maintain,
    // test and extend as later POE parts are developed.
    //
    // Reference:
    // Microsoft Learn (n.d.) TableClient Class - Azure.Data.Tables.
    public interface IMenuItemRepository
    {
        // Creates a new menu item in the MenuItems Azure Table.
        //
        // The MenuItemEntity contains:
        // PartitionKey = menu category
        // RowKey       = unique menu item ID / SKU
        //
        // The concrete repository will use TableClient.AddEntityAsync()
        // so that attempting to create an entity with the same
        // PartitionKey + RowKey can be detected as a duplicate.
        //
        // This operation supports:
        // POST /api/menu
        //
        // Reference:
        // Microsoft Learn (n.d.) TableClient.AddEntityAsync Method.
        Task CreateAsync(
            MenuItemEntity entity,
            CancellationToken cancellationToken = default);


        // Retrieves every menu item currently stored in the MenuItems table.
        //
        // The concrete repository will use an asynchronous Azure Table query
        // and return the results as a read-only collection.
        //
        // This operation supports:
        // GET /api/menu
        //
        // Reference:
        // Microsoft Learn (n.d.) TableClient Class - Azure.Data.Tables.
        Task<IReadOnlyList<MenuItemEntity>> GetAllAsync(
            CancellationToken cancellationToken = default);


        // Retrieves all menu items belonging to a specific category.
        //
        // Because CoffeeNChill stores Category as the PartitionKey,
        // the repository can query entities belonging to that partition.
        //
        // Example:
        // category = "Hot Drinks"
        //
        // This operation supports:
        // GET /api/menu/category/{category}
        //
        // This method is included in the shared repository contract so
        // Member 2 can build the category-filtering HTTP Function without
        // duplicating Azure Table Storage access logic.
        Task<IReadOnlyList<MenuItemEntity>> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default);


        // Retrieves one specific menu item using its Azure Table keys.
        //
        // category -> PartitionKey
        // id       -> RowKey
        //
        // The PartitionKey and RowKey together uniquely identify
        // an Azure Table entity.
        //
        // This method is required by the future update and delete endpoints
        // so that the application can confirm an item exists before changing it.
        //
        // Reference:
        // Microsoft Learn (2026) Azure Tables .NET quickstart.
        Task<MenuItemEntity?> GetByIdAsync(
            string category,
            string id,
            CancellationToken cancellationToken = default);


        // Updates an existing MenuItemEntity in Azure Table Storage.
        //
        // CoffeeNChill Part 1 requires menu item price and availability
        // to be updateable.
        //
        // The concrete repository will use the Azure.Data.Tables update
        // functionality rather than placing storage logic inside the
        // HTTP Function itself.
        //
        // This operation supports:
        // PUT /api/menu/{category}/{id}
        //
        // Reference:
        // Microsoft Learn (n.d.) TableClient.UpdateEntityAsync Method.
        Task UpdateAsync(
            MenuItemEntity entity,
            CancellationToken cancellationToken = default);


        // Removes an existing menu item from the MenuItems Azure Table.
        //
        // The category and ID are used as the PartitionKey and RowKey
        // required to identify the entity.
        //
        // This operation supports:
        // DELETE /api/menu/{category}/{id}
        Task DeleteAsync(
            string category,
            string id,
            CancellationToken cancellationToken = default);
    }
}