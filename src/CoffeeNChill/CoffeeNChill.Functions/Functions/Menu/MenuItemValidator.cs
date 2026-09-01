// Kaden Remley ST10472838
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// Validation/MenuItemValidator.cs

using CoffeeNChill.Functions.DTOs;

namespace CoffeeNChill.Functions.Validation
{
    // Centralises the route-value and field validation rules that are
    // repeated across the CoffeeNChill menu Functions (Create, filter,
    // update, delete).
    //
    // Why this exists:
    //
    // Before this class, "Category is required" and "Menu item ID / SKU
    // is required" were written out by hand inside every single Function
    // (CreateMenuItem, UpdateMenuItem, DeleteMenuItem all repeat the same
    // four lines). That's a maintenance risk: if the group ever changes
    // the wording of an error message, or the max length of a category
    // name, someone has to remember to change it in four different files.
    // Miss one and the API starts returning inconsistent error text for
    // the same underlying problem - which is exactly the kind of thing
    // the Part 1 rubric checks under "clear JSON error messages" and
    // "shared validation/error response approach used consistently".
    //
    // Each method returns an ErrorResponse when the value is invalid,
    // or null when it is valid. Returning null-for-valid (instead of a
    // bool) means the calling Function can do:
    //
    //   var error = MenuItemValidator.ValidateCategory(category);
    //   if (error != null) return new BadRequestObjectResult(error);
    //
    // in one line, instead of duplicating the ErrorResponse construction
    // at every call site.
    //
    // This class only builds the new GetMenuItemsByCategory function
    // (Member 2's endpoint) in Part 1. It does not modify Mohammed's
    // existing Create/Update/Delete Functions - adopting it there is a
    // follow-up group decision, not something to change unilaterally in
    // someone else's reviewed and merged code.
    public static class MenuItemValidator
    {
        private const int MaxCategoryLength = 100;
        private const int MaxIdLength = 50;

        // Validates a category value taken from a route segment
        // (e.g. GET /api/menu/category/{category}).
        //
        // A category maps directly to the Azure Table PartitionKey, so
        // an empty or whitespace-only value can never correspond to a
        // real partition - rejecting it early avoids sending a pointless
        // query to Azure Table Storage.
        public static ErrorResponse? ValidateCategoryRouteValue(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return new ErrorResponse
                {
                    Error = "VALIDATION_ERROR",
                    Message = "Category is required."
                };
            }

            if (category.Trim().Length > MaxCategoryLength)
            {
                return new ErrorResponse
                {
                    Error = "VALIDATION_ERROR",
                    Message = $"Category cannot exceed {MaxCategoryLength} characters."
                };
            }

            return null;
        }

        // Validates a menu item ID / SKU value taken from a route segment
        // (e.g. PUT /api/menu/{category}/{id}).
        //
        // This mirrors ValidateCategoryRouteValue but is kept as a
        // separate method rather than a single "ValidateRouteValue(string)"
        // helper, because category and ID are conceptually different
        // fields (PartitionKey vs RowKey) that may need different rules
        // in future (e.g. an ID format check) without affecting category
        // validation.
        public static ErrorResponse? ValidateIdRouteValue(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new ErrorResponse
                {
                    Error = "VALIDATION_ERROR",
                    Message = "Menu item ID / SKU is required."
                };
            }

            if (id.Trim().Length > MaxIdLength)
            {
                return new ErrorResponse
                {
                    Error = "VALIDATION_ERROR",
                    Message = $"Menu item ID / SKU cannot exceed {MaxIdLength} characters."
                };
            }

            return null;
        }

        // Validates a price value where the property is required
        // (used by creation-style flows). A price of exactly 0 is
        // treated as invalid, matching Mohammed's existing
        // CreateMenuItem behaviour (request.Price <= 0), so that a
        // menu item can never be listed as free by omission.
        public static ErrorResponse? ValidatePrice(double price)
        {
            if (price <= 0)
            {
                return new ErrorResponse
                {
                    Error = "VALIDATION_ERROR",
                    Message = "Price must be greater than zero."
                };
            }

            return null;
        }

        // Validates an optional price value where the property may be
        // omitted entirely (used by partial-update flows such as
        // UpdateMenuItemRequest.Price, which is a nullable double).
        //
        // A null value means "the client did not ask to change the
        // price" and is therefore valid by definition; only a supplied
        // non-positive value is rejected.
        public static ErrorResponse? ValidateOptionalPrice(double? price)
        {
            if (price.HasValue && price.Value <= 0)
            {
                return new ErrorResponse
                {
                    Error = "VALIDATION_ERROR",
                    Message = "Price must be greater than zero."
                };
            }

            return null;
        }
    }
}
