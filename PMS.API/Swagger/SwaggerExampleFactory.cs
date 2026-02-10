using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace PMS.API.Swagger
{
    internal static class SwaggerExampleFactory
    {
        internal static IOpenApiAny CreateExample(Type type)
        {
            var visited = new HashSet<Type>();
            return CreateExampleInternal(UnwrapNullable(type), visited, depth: 0);
        }

        private static IOpenApiAny CreateExampleInternal(Type type, HashSet<Type> visited, int depth)
        {
            type = UnwrapNullable(type);

            // Prevent cycles / overly deep graphs
            if (depth > 4)
            {
                return new OpenApiNull();
            }

            if (!visited.Add(type))
            {
                return new OpenApiNull();
            }

            try
            {
                // string
                if (type == typeof(string))
                {
                    return new OpenApiString("string");
                }

                // bool
                if (type == typeof(bool))
                {
                    return new OpenApiBoolean(true);
                }

                // numbers
                if (type == typeof(int) || type == typeof(short) || type == typeof(byte))
                {
                    return new OpenApiInteger(1);
                }

                if (type == typeof(long))
                {
                    return new OpenApiLong(1);
                }

                if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                {
                    return new OpenApiDouble(1.0);
                }

                // dates
                if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
                {
                    return new OpenApiString("2026-02-06T10:15:30Z");
                }

                // Guid
                if (type == typeof(Guid))
                {
                    return new OpenApiString("9f0c9a1b-1234-4cde-9876-abcdef123456");
                }

                // enums
                if (type.IsEnum)
                {
                    var names = Enum.GetNames(type);
                    return new OpenApiString(names.Length > 0 ? names[0] : "Unknown");
                }

                // IDictionary
                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    return new OpenApiObject();
                }

                // IEnumerable<T>
                if (TryGetEnumerableElementType(type, out var elementType))
                {
                    var arr = new OpenApiArray();
                    arr.Add(CreateExampleInternal(elementType, visited, depth + 1));
                    return arr;
                }

                // complex object
                var obj = new OpenApiObject();
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    var propType = UnwrapNullable(prop.PropertyType);
                    obj[ToCamelCase(prop.Name)] = CreatePropertyExample(prop.Name, propType, visited, depth + 1);
                }

                return obj;
            }
            finally
            {
                visited.Remove(type);
            }
        }

        private static IOpenApiAny CreatePropertyExample(string propertyName, Type propType, HashSet<Type> visited, int depth)
        {
            var name = propertyName.ToLowerInvariant();

            if (propType == typeof(string))
            {
                if (name is "id" or "userid" or "employeeid")
                {
                    return new OpenApiString("9f0c9a1b-1234-4cde-9876-abcdef123456");
                }

                if (name.Contains("email", StringComparison.Ordinal))
                {
                    return new OpenApiString("user@example.com");
                }

                if (name.Contains("phone", StringComparison.Ordinal))
                {
                    return new OpenApiString("+201234567890");
                }

                if (name.Contains("username", StringComparison.Ordinal))
                {
                    return new OpenApiString("username");
                }

                if (name.Contains("fullname", StringComparison.Ordinal) || name.Contains("name", StringComparison.Ordinal))
                {
                    return new OpenApiString("Ahmed Ali");
                }

                if (name.Contains("token", StringComparison.Ordinal))
                {
                    return new OpenApiString("jwt_token_here");
                }

                return new OpenApiString("string");
            }

            if (propType == typeof(DateTime) || propType == typeof(DateTimeOffset))
            {
                return new OpenApiString("2026-02-06T10:15:30Z");
            }

            return CreateExampleInternal(propType, visited, depth);
        }

        internal static OpenApiObject CreateApiResponseErrorExample(string message)
        {
            return new OpenApiObject
            {
                ["succeeded"] = new OpenApiBoolean(false),
                ["message"] = new OpenApiString(message),
                ["errors"] = new OpenApiArray { new OpenApiString(message) },
                ["data"] = new OpenApiNull()
            };
        }

        internal static OpenApiObject CreateApiResponseSuccessExample(IOpenApiAny dataExample, string? message = null)
        {
            return new OpenApiObject
            {
                ["succeeded"] = new OpenApiBoolean(true),
                ["message"] = new OpenApiString(message ?? "Operation successful"),
                ["errors"] = new OpenApiNull(),
                ["data"] = dataExample
            };
        }

        internal static OpenApiObject CreatePagedResultExample(IOpenApiAny elementExample)
        {
            return new OpenApiObject
            {
                ["data"] = new OpenApiArray { elementExample },
                ["totalCount"] = new OpenApiInteger(42),
                ["pageNumber"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(10),
                ["totalPages"] = new OpenApiInteger(5)
            };
        }

        /// <summary>
        /// Error example for ResponseObjectDto. Always isSuccess: false so 4xx/5xx never show success in Swagger.
        /// </summary>
        internal static OpenApiObject CreateResponseObjectDtoErrorExample(int statusCode, string message)
        {
            if (statusCode <= 0 || statusCode >= 600) statusCode = 400;
            return new OpenApiObject
            {
                ["isSuccess"] = new OpenApiBoolean(false),
                ["message"] = new OpenApiString(message ?? "Request failed"),
                ["data"] = new OpenApiNull(),
                ["statusCode"] = new OpenApiInteger(statusCode)
            };
        }

        /// <summary>
        /// Success example for ResponseObjectDto.
        /// </summary>
        internal static OpenApiObject CreateResponseObjectDtoSuccessExample(IOpenApiAny dataExample, int statusCode = 200, string? message = null)
        {
            return new OpenApiObject
            {
                ["isSuccess"] = new OpenApiBoolean(true),
                ["message"] = new OpenApiString(message ?? "Operation successful"),
                ["data"] = dataExample,
                ["statusCode"] = new OpenApiInteger(statusCode)
            };
        }

        private static bool TryGetEnumerableElementType(Type type, out Type elementType)
        {
            elementType = typeof(object);

            if (type == typeof(string))
            {
                return false;
            }

            if (type.IsArray)
            {
                elementType = type.GetElementType() ?? typeof(object);
                return true;
            }

            var ienum = type
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (ienum != null)
            {
                elementType = ienum.GetGenericArguments()[0];
                return true;
            }

            return false;
        }

        private static Type UnwrapNullable(Type type) => Nullable.GetUnderlyingType(type) ?? type;

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
            {
                return value;
            }

            // Keep acronyms readable: URLValue -> urlValue
            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var hasNext = i + 1 < chars.Length;
                if (i == 0 || (hasNext && char.IsUpper(chars[i + 1])))
                {
                    chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
                    continue;
                }

                break;
            }

            return new string(chars);
        }
    }
}

