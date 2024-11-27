using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApp.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.ModelMetadata?.ContainerType == typeof(Microsoft.AspNetCore.Http.IFormFile))
            .ToList();

        if (!fileParameters.Any())
        {
            return; // No file parameters, nothing to do
        }

        // Remove existing parameters for the file upload
        foreach (var parameter in fileParameters)
        {
            var existingParam = operation.Parameters.FirstOrDefault(p => p.Name == parameter.Name);
            if (existingParam != null)
            {
                operation.Parameters.Remove(existingParam);
            }
        }

        // Add a request body for multipart/form-data
        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileParameters.ToDictionary(
                            p => p.Name,
                            p => new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }),
                        Required = new HashSet<string>(fileParameters.Select(p => p.Name))
                    }
                }
            }
        };
    }
}