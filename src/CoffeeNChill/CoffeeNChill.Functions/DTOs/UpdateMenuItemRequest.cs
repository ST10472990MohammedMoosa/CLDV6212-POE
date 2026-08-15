// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// DTOs/UpdateMenuItemRequest.cs

namespace CoffeeNChill.Functions.DTOs
{
    // Represents the request body used when an existing menu item is updated.
    //
    // Required endpoint:
    // PUT /api/menu/{category}/{id}
    //
    // The CoffeeNChill Part 1 specification requires menu item price and
    // availability to be updateable.
    //
    // Nullable properties are deliberately used so that the application
    // can distinguish between:
    //
    // 1. A value supplied by the client.
    // 2. A value omitted from the request.
    //
    // This allows partial updates without unintentionally replacing
    // existing values.
    public class UpdateMenuItemRequest
    {
        // Optional replacement price.
        //
        // null = client did not request a price change.
        //
        // A supplied value will later be validated before the entity
        // is updated in Azure Table Storage.
        public double? Price { get; set; }


        // Optional replacement availability state.
        //
        // null  = client did not request an availability change.
        // true  = mark item as available.
        // false = mark item as unavailable.
        //
        // bool? is important here because a normal bool could not
        // distinguish "false" from "not provided".
        public bool? IsAvailable { get; set; }
    }
}