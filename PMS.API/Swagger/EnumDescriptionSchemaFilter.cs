using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace PMS.API.Swagger
{
    /// <summary>
    
    /// </summary>
    public class EnumDescriptionSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema == null || context.Type == null)
            {
                return;
            }

            var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
            if (!enumType.IsEnum)
            {
                return;
            }

            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType).Cast<int>().ToArray();

            var mapping = string.Join(", ", names.Select((n, i) => $"{values[i]} = {n}"));

            
            if (string.IsNullOrWhiteSpace(schema.Description))
            {
                schema.Description = $"Values: {mapping}";
            }
            else
            {
                schema.Description += $" (Values: {mapping})";
            }
        }
    }
}

