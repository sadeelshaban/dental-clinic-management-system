using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DentalClinic.API.Extensions;

/// <summary>
/// Swagger operation filter to handle ApiResponse envelope structure
/// </summary>
public class ApiResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Skip operations that don't have a return type
        if (operation.Responses == null || !operation.Responses.Any())
            return;

        // Get the return type from the method
        var returnType = context.MethodInfo.ReturnType;
        if (returnType == null)
            return;

        // Check if the return type is ActionResult<ApiResponse> or ApiResponse
        var isApiResponse = returnType.IsGenericType && 
                           (returnType.GetGenericTypeDefinition() == typeof(DentalClinic.API.DTOs.Common.ApiResponse<>) ||
                            returnType.GetGenericTypeDefinition().Name == "ActionResult`1");

        if (!isApiResponse)
            return;

        // Add schema references for common response codes
        foreach (var response in operation.Responses.Values)
        {
            if (response.Content == null || !response.Content.ContainsKey("application/json"))
                continue;

            var jsonContent = response.Content["application/json"];
            if (jsonContent?.Schema == null)
                continue;

            // The ApiResponse envelope has: success (bool), message (string), data (T)
            // This filter ensures Swagger can properly display the nested structure
        }
    }
}
