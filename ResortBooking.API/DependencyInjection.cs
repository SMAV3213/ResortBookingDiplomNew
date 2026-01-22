using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ResortBooking.API.Filters;
using ResortBooking.Application.Data.Options;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Infrastructure.Persistence;
using ResortBooking.Infrastructure.Repositories;
using ResortBooking.Infrastructure.Services;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace ResortBooking.API;

/// <summary>
/// Инъекция зависимостей.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddEndpointsApiExplorer()
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter()
                );
            });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });


        services.AddMemoryCache();
        services.AddApiAuthorization(configuration);
        services.AddApiSwagger();

        //Репозитории
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        //Сервисы
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoomTypeService, RoomTypeService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly(),
            ServiceLifetime.Scoped
        );

        return services;
    }

    public static IServiceCollection AddApiAuthorization(
    this IServiceCollection services, IConfiguration configuration
    )
    {
        using var provider = services.BuildServiceProvider();
        var jwtSettings = configuration.GetSection("Jwt");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
            };

            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    var response = context.Response;
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.ContentType = "text/plain; charset=utf-8";
                    var message = "Не авторизован";
                    await response.WriteAsync(message);
                }
            };
        });

        return services;
    }

    private static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            var currentAssembly = Assembly.GetExecutingAssembly();

            var xmlDocs = currentAssembly.GetReferencedAssemblies()
                .Union([currentAssembly.GetName()])
                .Select(a => Path.Combine(AppContext.BaseDirectory, $"{a.Name}.xml"))
                .Where(f => File.Exists(f)).ToList();

            xmlDocs.ForEach(xmlDoc => options.IncludeXmlComments(xmlDoc));

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "Введите JWT токен авторизации.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });

            //options.DocumentFilter<InfoFilter>();
            options.OperationFilter<JwtAuthorizeFilter>();
        });

        return services;
    }

    /// <summary>
    /// Добавление политик cors.
    /// </summary>
    public static IServiceCollection AddApiCors(
        this IServiceCollection services
    )
    {
        using var provider = services.BuildServiceProvider();
        var app = provider.GetRequiredService<IAppOptions>();

        services.AddCors(options =>
        {
            options.AddPolicy(
                "cors-policy",
                policy =>
                {
                    policy
                        .WithOrigins([.. app.CorsOrigins])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(3600))
                        .WithExposedHeaders("Content-Disposition");
                }
            );
        });

        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app,
    IWebHostEnvironment webHostEnvironment)
    {
        app.UseExceptionHandler(options => { });

        app.MapControllers();

        app.UseHttpsRedirection();
        app.UseAuth();
        app.UseStaticFiles();
        app.UseApiSwagger(webHostEnvironment);

        return app;
    }

    private static WebApplication UseAuth(this WebApplication app)
    {
        app.UseCors("cors-policy");
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    private static WebApplication UseApiSwagger(this WebApplication app,
    IWebHostEnvironment webHostEnvironment)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
            options.EnableDeepLinking();
            options.DocumentTitle =
                $"({(webHostEnvironment.IsProduction() ? "Prod" : "Dev")}) ResortBooking REST API";
        });

        return app;
    }
}
