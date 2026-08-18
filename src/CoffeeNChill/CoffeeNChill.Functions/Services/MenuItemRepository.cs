// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Services/MenuItemRepository.cs

using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Interfaces;
using CoffeeNChill.Functions.Models;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Functions.Services
{
    // Provides the Azure Table Storage implementation of IMenuItemRepository.
    //
    // This class contains the storage-specific logic for the CoffeeNChill
    // MenuItems table.
    //
    // HTTP Functions should not communicate directly with Azure Table Storage.
    // Instead, they communicate through IMenuItemRepository.
    //
    // Architecture:
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
    //      |
    //      v
    // MenuItems Table
    //
    // This provides separation of concerns between the API layer
    // and the storage layer.
    //
    // Reference:
    // Microsoft Learn (n.d.) TableClient Class - Azure.Data.Tables.
    public class MenuItemRepository : IMenuItemRepository
    {
        // Name of the Azure Table required by the CoffeeNChill POE.
        //
        // Keeping the name in one constant prevents the table name
        // from being repeated throughout the repository.
        private const string TableName = "MenuItems";


        // Azure SDK client used to communicate with one Azure Table.
        //
        // During local Part 1 development this client will connect
        // to Azurite through the AzureWebJobsStorage configuration value.
        private readonly TableClient _tableClient;


        // IConfiguration is supplied through .NET dependency injection.
        //
        // AzureWebJobsStorage is read from the application's configuration.
        //
        // During local development the Azure Functions template uses:
        //
        // AzureWebJobsStorage = UseDevelopmentStorage=true
        //
        // which allows the application to use the local Azurite emulator.
        //
        // The connection string itself is not hardcoded into source code.
        //
        // Reference:
        // Microsoft Learn (2026) Develop and run Azure Functions locally.
        public MenuItemRepository(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureWebJobsStorage"]
                ?? throw new InvalidOperationException(
                    "The AzureWebJobsStorage configuration value is missing.");

            // Creates a TableClient configured for the CoffeeNChill MenuItems table.
            //
            // TableClient supports both Azure Storage Tables and the
            // Azurite Table Storage emulator when an appropriate connection
            // string is supplied.
            _tableClient = new TableClient(
                connectionString,
                TableName);
        }


        // Ensures that the MenuItems table exists before an operation
        // attempts to use it.
        //
        // CreateIfNotExistsAsync is safe to call even when the table already
        // exists. This is useful during local development because a new
        // Azurite instance may initially contain no tables.
        //
        // Reference:
        // Microsoft Learn (n.d.) TableClient.CreateIfNotExistsAsync Method.
        private async Task EnsureTableExistsAsync(
            CancellationToken cancellationToken = default)
        {
            await _tableClient.CreateIfNotExistsAsync(
                cancellationToken);
        }


        // Creates a new entity in the MenuItems table.
        //
        // AddEntityAsync is used rather than UpsertEntityAsync.
        //
        // This is deliberate:
        //
        // AddEntityAsync:
        // - creates a new entity
        // - fails if the PartitionKey + RowKey already exists
        //
        // UpsertEntityAsync:
        // - inserts when missing
        // - updates/replaces when already present
        //
        // For POST /api/menu we want duplicate menu items to be detected
        // instead of silently overwriting an existing item.
        //
        // The HTTP Function will later translate duplicate storage errors
        // into an appropriate API response such as 409 Conflict.
        //
        // Reference:
        // Microsoft Learn (n.d.) TableClient.AddEntityAsync Method.
        public async Task CreateAsync(
            MenuItemEntity entity,
            CancellationToken cancellationToken = default)
        {
            await EnsureTableExistsAsync(cancellationToken);

            await _tableClient.AddEntityAsync(
                entity,
                cancellationToken);
        }


        // Retrieves every entity currently stored in MenuItems.
        //
        // QueryAsync returns AsyncPageable<MenuItemEntity>.
        //
        // Azure Table queries may return results in multiple pages,
        // therefore await foreach is used to asynchronously enumerate
        // all returned entities.
        //
        // Reference:
        // Microsoft Learn (n.d.) TableClient.QueryAsync Method.
        public async Task<IReadOnlyList<MenuItemEntity>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await EnsureTableExistsAsync(cancellationToken);

            var menuItems = new List<MenuItemEntity>();

            await foreach (
                MenuItemEntity entity in
                _tableClient.QueryAsync<MenuItemEntity>(
                    cancellationToken: cancellationToken))
            {
                menuItems.Add(entity);
            }

            return menuItems;
        }


        // Retrieves all menu items belonging to a requested category.
        //
        // CoffeeNChill stores:
        //
        // Category -> PartitionKey
        //
        // Therefore querying PartitionKey allows all entities within
        // the requested menu category to be returned.
        //
        // TableClient.CreateQueryFilter is used instead of manually
        // constructing an OData filter string.
        //
        // The Azure SDK automatically quotes and escapes interpolated
        // values as required when constructing the OData expression.
        //
        // Example:
        // category = "Hot Drinks"
        //
        // resulting logical filter:
        // PartitionKey eq 'Hot Drinks'
        //
        // This operation supports:
        // GET /api/menu/category/{category}
        //
        // Reference:
        // Microsoft Learn (n.d.) TableClient.CreateQueryFilter Method.
        public async Task<IReadOnlyList<MenuItemEntity>> GetByCategoryAsync(
            string category,
            CancellationToken cancellationToken = default)
        {
            await EnsureTableExistsAsync(cancellationToken);

            var menuItems = new List<MenuItemEntity>();

            string filter = TableClient.CreateQueryFilter(
                $"PartitionKey eq {category}");

            await foreach (
                MenuItemEntity entity in
                _tableClient.QueryAsync<MenuItemEntity>(
                    filter: filter,
                    cancellationToken: cancellationToken))
            {
                menuItems.Add(entity);
            }

            return menuItems;
        }


        // Retrieves one menu item using its composite Azure Table key.
        //
        // CoffeeNChill mapping:
        //
        // category -> PartitionKey
        // id       -> RowKey
        //
        // GetEntityIfExistsAsync is used because a missing menu item is
        // a normal API scenario and should be represented as null instead
        // of forcing the calling HTTP Function to handle a storage exception.
        //
        // This method will later support update and delete operations.
        public async Task<MenuItemEntity?> GetByIdAsync(
            string category,
            string id,
            CancellationToken cancellationToken = default)
        {
            await EnsureTableExistsAsync(cancellationToken);

            NullableResponse<MenuItemEntity> response =
                await _tableClient.GetEntityIfExistsAsync<MenuItemEntity>(
                    partitionKey: category,
                    rowKey: id,
                    cancellationToken: cancellationToken);

            if (!response.HasValue)
            {
                return null;
            }

            return response.Value;
        }


        // Updates an existing MenuItems entity.
        //
        // TableUpdateMode.Merge is used so that supplied property values
        // are merged with the existing Azure Table entity.
        //
        // The entity's ETag is also supplied.
        //
        // ETags provide concurrency information and help prevent a client
        // from blindly overwriting an entity that changed after it was read.
        //
        // Member 2 will later:
        //
        // 1. Retrieve the existing entity.
        // 2. Change Price and/or IsAvailable.
        // 3. Pass the entity into this method.
        //
        // This operation supports:
        // PUT /api/menu/{category}/{id}
        //
        // Reference:
        // Microsoft Learn (n.d.) TableClient.UpdateEntityAsync Method.
        public async Task UpdateAsync(
            MenuItemEntity entity,
            CancellationToken cancellationToken = default)
        {
            await EnsureTableExistsAsync(cancellationToken);

            await _tableClient.UpdateEntityAsync(
                entity,
                entity.ETag,
                TableUpdateMode.Merge,
                cancellationToken);
        }


        // Deletes a menu item using its Azure Table PartitionKey and RowKey.
        //
        // category -> PartitionKey
        // id       -> RowKey
        //
        // ETag.All represents a wildcard ETag for this delete operation.
        //
        // Member 2 will later perform existence checks and error handling
        // in the HTTP Function layer before calling this repository method.
        //
        // This operation supports:
        // DELETE /api/menu/{category}/{id}
        //
        // Reference:
        // Microsoft Learn (n.d.) TableClient.DeleteEntityAsync Method.
        public async Task DeleteAsync(
            string category,
            string id,
            CancellationToken cancellationToken = default)
        {
            await EnsureTableExistsAsync(cancellationToken);

            await _tableClient.DeleteEntityAsync(
                partitionKey: category,
                rowKey: id,
                ifMatch: ETag.All,
                cancellationToken: cancellationToken);
        }
    }
}