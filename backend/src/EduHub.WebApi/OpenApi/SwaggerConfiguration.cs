using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace EduHub.WebApi.OpenApi;

/// <summary>
/// Ghi chú: SwaggerConfiguration cấu hình mapping/constraint/index cho Swagger/OpenAPI của API.
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Ghi chú: AddEduHubSwagger thực hiện phần xử lý của Swagger/OpenAPI của API.
    /// </summary>
    public static IServiceCollection AddEduHubSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EduHub API",
                Version = "v1",
                Description = "EduHub student-management API."
            });
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                Description = "Send a JWT Bearer token."
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document, externalResource: null)
                ] = []
            });
            options.MapType<ProblemDetails>(() => new OpenApiSchema
            {
                Type = JsonSchemaType.Object
            });
        });

        return services;
    }
}
