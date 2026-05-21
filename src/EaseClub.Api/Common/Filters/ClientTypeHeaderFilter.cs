using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EaseClub.Api.Common.Filters
{
    public class ClientTypeHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAttribute = context.MethodInfo.GetCustomAttributes(true).OfType<RequireClientTypeAttribute>().Any();
            if (!hasAttribute) return;

            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Client-Type",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Must be 'Web' or 'Mobile'",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny>
                {
                    new OpenApiString("Web"),
                    new OpenApiString("Mobile")
                }
                }
            });
        }
    }

    //for swagger to recognize the attribute and apply the filter only to methods or classes decorated with it
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireClientTypeAttribute : Attribute { }
}
