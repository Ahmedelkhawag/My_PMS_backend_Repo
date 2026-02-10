using Microsoft.OpenApi.Models;
using PMS.Application.DTOs.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace PMS.API.Swagger
{
    public class GlobalExamplesOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Only 2xx is success. 0, 1xx, 3xx, 4xx, 5xx must never show isSuccess/succeeded: true in examples.
        /// </summary>
        private static bool IsSuccessStatusCode(int statusCode) => statusCode >= 200 && statusCode < 300;

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
                var statusCodeInt = supported.StatusCode;

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

                // Critical: only 2xx gets success example. Default response and any non-2xx get error example.
                var useErrorExample = supported.IsDefaultResponse || !IsSuccessStatusCode(statusCodeInt);

                // If response is ApiResponse<T>, generate consistent success/error example.
                if (IsGenericTypeDefinition(clrType, typeof(ApiResponse<>), out var apiArg))
                {
                    if (useErrorExample)
                    {
                        var message = GetDefaultMessageForStatusCode(statusCodeInt);
                        mediaType.Example = SwaggerExampleFactory.CreateApiResponseErrorExample(message);
                    }
                    else
                    {
                        var dataExample = SwaggerExampleFactory.CreateExample(apiArg);
                        mediaType.Example = SwaggerExampleFactory.CreateApiResponseSuccessExample(dataExample);
                    }

                    continue;
                }

                // If response is ResponseObjectDto<T>, generate success or error example.
                if (IsGenericTypeDefinition(clrType, typeof(ResponseObjectDto<>), out var responseDtoArg))
                {
                    if (useErrorExample)
                    {
                        var message = GetDefaultMessageForStatusCode(statusCodeInt);
                        var code = statusCodeInt > 0 ? statusCodeInt : 400;
                        mediaType.Example = SwaggerExampleFactory.CreateResponseObjectDtoErrorExample(code, message);
                    }
                    else
                    {
                        var dataExample = SwaggerExampleFactory.CreateExample(responseDtoArg);
                        var code = statusCodeInt;
                        mediaType.Example = SwaggerExampleFactory.CreateResponseObjectDtoSuccessExample(dataExample, code, "Operation successful");
                    }

                    continue;
                }

                // If response is PagedResult<T>: only 2xx gets paged example; 4xx/5xx must show error.
                if (IsGenericTypeDefinition(clrType, typeof(PagedResult<>), out var pageArg))
                {
                    if (useErrorExample)
                    {
                        var message = GetDefaultMessageForStatusCode(statusCodeInt);
                        var code = statusCodeInt > 0 ? statusCodeInt : 400;
                        mediaType.Example = SwaggerExampleFactory.CreateResponseObjectDtoErrorExample(code, message);
                    }
                    else
                    {
                        var elementExample = SwaggerExampleFactory.CreateExample(pageArg);
                        mediaType.Example = SwaggerExampleFactory.CreatePagedResultExample(elementExample);
                    }
                    continue;
                }

                // Other types: for 4xx/5xx (or 0) never show a success-looking body — use error example.
                if (useErrorExample)
                {
                    var message = GetDefaultMessageForStatusCode(statusCodeInt);
                    var code = statusCodeInt > 0 ? statusCodeInt : 400;
                    mediaType.Example = SwaggerExampleFactory.CreateResponseObjectDtoErrorExample(code, message);
                }
                else
                {
                    mediaType.Example = SwaggerExampleFactory.CreateExample(clrType);
                }
            }

            // Ensure common error responses exist and have examples.
            EnsureErrorResponseWithExample(operation, 400, "Bad Request");
            EnsureErrorResponseWithExample(operation, 401, "Unauthorized");
            EnsureErrorResponseWithExample(operation, 403, "Forbidden");
            EnsureErrorResponseWithExample(operation, 404, "Not Found");
            EnsureErrorResponseWithExample(operation, 409, "Conflict");
            EnsureErrorResponseWithExample(operation, 500, "Internal Server Error");
        }

        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                409 => "Conflict",
                500 => "Internal Server Error",
                _ => "Request failed"
            };
        }

        private static void EnsureErrorResponseWithExample(OpenApiOperation operation, int statusCode, string message)
        {
            var statusKey = statusCode.ToString();
            if (!operation.Responses.TryGetValue(statusKey, out var response))
            {
                operation.Responses[statusKey] = response = new OpenApiResponse
                {
                    Description = message
                };
            }

            if (response.Content == null || !response.Content.ContainsKey("application/json"))
            {
                response.Content ??= new Dictionary<string, OpenApiMediaType>();
                response.Content["application/json"] = new OpenApiMediaType();
            }

            var mediaType = response.Content["application/json"];
            // Always set error example for 4xx/5xx so no error status ever shows a success body.
            if (mediaType.Example == null)
            {
                mediaType.Example = SwaggerExampleFactory.CreateResponseObjectDtoErrorExample(statusCode, message);
            }
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

