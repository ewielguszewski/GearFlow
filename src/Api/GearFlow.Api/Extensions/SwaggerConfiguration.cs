using Microsoft.OpenApi.Models;
using System.Reflection;

namespace GearFlow.Api.Extensions;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GearFlow API",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                              "Wpisz: **{token}** (bez 'Bearer', bez nawiasów).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });
            c.OperationFilter<AuthorizeOperationFilter>();
            c.SupportNonNullableReferenceTypes();
        });

        return services;
    }
}
