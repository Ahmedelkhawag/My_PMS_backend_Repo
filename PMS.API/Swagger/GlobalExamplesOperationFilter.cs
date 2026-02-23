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
            if (operation == null) return;

            foreach (var supported in context.ApiDescription.SupportedResponseTypes)
            {
                var statusCode = supported.StatusCode.ToString();
                var statusCodeInt = supported.StatusCode;

                if (!operation.Responses.TryGetValue(statusCode, out var response)) continue;

                // التعديل الجوهري: هنلف على كل أنواع الـ Content (json, text/plain, etc)
                foreach (var content in response.Content)
                {
                    var mediaType = content.Value;

                    // لو فيه مثال يدوي محطوط، مش هنلمسه
                    if (mediaType.Example != null) continue;

                    var clrType = supported.Type ?? supported.ModelMetadata?.ModelType;
                    if (clrType == null) continue;

                    var useErrorExample = supported.IsDefaultResponse || !IsSuccessStatusCode(statusCodeInt);

                    // نركز فقط على الـ ResponseObjectDto لتوحيد السيستم
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
                            mediaType.Example = SwaggerExampleFactory.CreateResponseObjectDtoSuccessExample(dataExample, statusCodeInt, "Operation successful");
                        }
                    }
                    // لو paged result
                    else if (IsGenericTypeDefinition(clrType, typeof(PagedResult<>), out var pageArg))
                    {
                        if (useErrorExample)
                        {
                            var message = GetDefaultMessageForStatusCode(statusCodeInt);
                            mediaType.Example = SwaggerExampleFactory.CreateResponseObjectDtoErrorExample(statusCodeInt > 0 ? statusCodeInt : 400, message);
                        }
                        else
                        {
                            var elementExample = SwaggerExampleFactory.CreateExample(pageArg);
                            mediaType.Example = SwaggerExampleFactory.CreatePagedResultExample(elementExample);
                        }
                    }
                    // أي نوع تاني وفي حالة فشل، اجبره يظهر شكل الـ Error بتاعنا
                    else if (useErrorExample)
                    {
                        var message = GetDefaultMessageForStatusCode(statusCodeInt);
                        mediaType.Example = SwaggerExampleFactory.CreateResponseObjectDtoErrorExample(statusCodeInt > 0 ? statusCodeInt : 400, message);
                    }
                }
            }

            // نضمن إن الإيرورز المشهورة ليها أمثلة في السواجر
            EnsureErrorResponseWithExample(operation, 400, "Bad Request");
            EnsureErrorResponseWithExample(operation, 401, "Unauthorized");
            EnsureErrorResponseWithExample(operation, 403, "Forbidden");
            EnsureErrorResponseWithExample(operation, 404, "Not Found");
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

