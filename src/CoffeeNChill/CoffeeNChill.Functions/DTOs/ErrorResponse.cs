// Mohammed Moosa ST10472990
// CLDV6212 Cloud Development B
// POE Part 1 - CoffeeNChill Canteen Management System
// DTOs/ErrorResponse.cs

namespace CoffeeNChill.Functions.DTOs
{
    // Provides a consistent response structure for API errors.
    //
    // CoffeeNChill endpoints will use this DTO when returning errors
    // such as:
    //
    // 400 Bad Request
    // 404 Not Found
    // 409 Conflict
    // 500 Internal Server Error
    //
    // Returning structured JSON errors is clearer for Postman tests
    // and API clients than returning unrelated plain-text messages.
    public class ErrorResponse
    {
        // Short machine/readable error category.
        //
        // Example:
        // "VALIDATION_ERROR"
        // "MENU_ITEM_NOT_FOUND"
        // "DUPLICATE_MENU_ITEM"
        public string Error { get; set; } = string.Empty;


        // Human-readable explanation of the problem.
        //
        // Example:
        // "Price cannot be negative."
        public string Message { get; set; } = string.Empty;
    }
}