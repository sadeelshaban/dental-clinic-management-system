namespace DentalClinic.API.Common;

/// <summary>
/// Thrown when a request violates a business rule (duplicate email, unsafe role change,
/// last-admin protection, etc.). Translated to HTTP 400 with the standard ApiResponse
/// envelope by the ExceptionHandlingMiddleware.
/// </summary>
public class BusinessRuleException(string message) : Exception(message);