using Microsoft.OpenApi.Models;
using PMS.Application.DTOs.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace PMS.API.Swagger
{
    public class GlobalExamplesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null)
            {
                return;
            }

            // Use ApiExplorer to discover actual CLR response types per status code.
            foreach (var supported in context.ApiDescription.SupportedResponseTypes)
            {
                var statusCode = supported.StatusCode.ToString();

                if (!operation.Responses.TryGetValue(statusCode, out var response))
                {
                    continue;
                }

                if (!response.Content.TryGetValue("application/json", out var mediaType))
                {
                    continue;
                }

                // Don't overwrite explicitly provided examples.
                if (mediaType.Example != null)
                {
                    continue;
                }

                var clrType = supported.Type ?? supported.ModelMetadata?.ModelType;
                if (clrType == null)
                {
                    continue;
                }

                // If response is ApiResponse<T>, generate consistent success/error example.
                if (IsGenericTypeDefinition(clrType, typeof(ApiResponse<>), out var apiArg))
                {
                    if (supported.IsDefaultResponse || supported.StatusCode >= 400)
                    {
                        mediaType.Example = SwaggerExampleFactory.CreateApiResponseErrorExample("Request failed");
                    }
                    else
                    {
                        var dataExample = SwaggerExampleFactory.CreateExample(apiArg);
                        mediaType.Example = SwaggerExampleFactory.CreateApiResponseSuccessExample(dataExample);
                    }

                    continue;
                }

                // If response is PagedResult<T>, generate paged example.
                if (IsGenericTypeDefinition(clrType, typeof(PagedResult<>), out var pageArg))
                {
                    var elementExample = SwaggerExampleFactory.CreateExample(pageArg);
                    mediaType.Example = SwaggerExampleFactory.CreatePagedResultExample(elementExample);
                    continue;
                }

                // Other types (DTOs, lists, primitives)
                mediaType.Example = SwaggerExampleFactory.CreateExample(clrType);
            }

            // Ensure common error responses have examples even if ApiExplorer didn't specify types.
            AddFallbackErrorExample(operation, "400", "Bad Request");
            AddFallbackErrorExample(operation, "401", "Unauthorized");
            AddFallbackErrorExample(operation, "403", "Forbidden");
            AddFallbackErrorExample(operation, "404", "Not Found");
            AddFallbackErrorExample(operation, "500", "Server Error");
        }

        private static void AddFallbackErrorExample(OpenApiOperation operation, string statusCode, string message)
        {
            if (!operation.Responses.TryGetValue(statusCode, out var response))
            {
                return;
            }

            if (!response.Content.TryGetValue("application/json", out var mediaType))
            {
                return;
            }

            if (mediaType.Example != null)
            {
                return;
            }

            mediaType.Example = SwaggerExampleFactory.CreateApiResponseErrorExample(message);
        }

        private static bool IsGenericTypeDefinition(Type type, Type openGeneric, out Type arg)
        {
            arg = typeof(object);

            if (!type.IsGenericType)
            {
                return false;
            }

            if (type.GetGenericTypeDefinition() != openGeneric)
            {
                return false;
            }

            arg = type.GetGenericArguments()[0];
            return true;
        }
    }
}

