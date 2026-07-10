using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GearFlow.Api.Extensions;

public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodInfo = context.MethodInfo;

        var hasAuthorizeAttribute = methodInfo.DeclaringType?.GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any() == true
            || methodInfo.GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();

        var hasAllowAnonymousAttribute = methodInfo.GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any();

        if (!hasAuthorizeAttribute || hasAllowAnonymousAttribute)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}
