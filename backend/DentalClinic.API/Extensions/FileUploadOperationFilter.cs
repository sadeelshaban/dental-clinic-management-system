using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DentalClinic.API.Extensions;

/// <summary>
/// Swagger operation filter to handle file upload endpoints with IFormFile
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileUploadMime = "multipart/form-data";
        if (operation.RequestBody == null || !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))
            return;

        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile) ||
                       p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFileCollection));

        if (fileParams.Count() > 0)
        {
            operation.RequestBody.Content[fileUploadMime].Schema.Properties =
                fileParams.ToDictionary(k => k.Name ?? "file", v => new OpenApiSchema()
                {
                    Type = "string",
                    Format = "binary"
                });
        }
    }
}
