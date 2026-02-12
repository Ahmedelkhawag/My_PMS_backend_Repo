using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace PMS.API.Swagger
{
    /// <summary>
    /// يضيف وصف تلقائي لكل Enum في Swagger يبين قيمة كل رقم.
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

            // لو فيه description قديم (من XML) نزود عليه بدل ما نمسحه
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

