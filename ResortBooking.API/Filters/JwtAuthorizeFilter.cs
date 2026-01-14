using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ResortBooking.API.Filters;

/// <summary>
/// Фильтр операции Swagger.
/// </summary>
public class JwtAuthorizeFilter : IOperationFilter
{
    /// <summary>
    /// Схема.
    /// </summary>
    public void Apply(OpenApiOperation operation,
        OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType is null)
            return;

        var attributesAuthorize = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .ToList();

        if (attributesAuthorize.Count == 0)
        {
            attributesAuthorize = context.MethodInfo.DeclaringType
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .ToList();
        }

        // Проверка на аттрибут Authorize

        if (attributesAuthorize.Count == 0)
            return;

        var scheme = new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, context.Document);

        operation.Security =
        [
            new OpenApiSecurityRequirement
                {
                    [scheme] = new List<string>()
                }
        ];
    }
}