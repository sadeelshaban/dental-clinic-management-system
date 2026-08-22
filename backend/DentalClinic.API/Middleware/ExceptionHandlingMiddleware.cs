using System.Net;
using System.Text.Json;
using DentalClinic.API.Common;
using DentalClinic.API.DTOs.Common;

namespace DentalClinic.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessRuleException ex)
        {
            logger.LogWarning("Business rule violation for {Method} {Path}: {Message}",
                context.Request.Method,
                context.Request.Path,
                ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var response = ApiResponse<object>.Fail(ex.Message);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = ApiResponse<object>.Fail(
                environment.IsDevelopment()
                    ? ex.Message
                    : "An unexpected error occurred.");

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
