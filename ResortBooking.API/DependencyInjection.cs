using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
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
using ResortBooking.Application.Validators.UserValidators;
using ResortBooking.Infrastructure.BackgroundServices;
using ResortBooking.Infrastructure.Persistence;
using ResortBooking.Infrastructure.Repositories;
using ResortBooking.Infrastructure.Services;

namespace ResortBooking.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddEndpointsApiExplorer()
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(typeof(UpdateUserDTOValidator).Assembly);

        services.AddMemoryCache();

        // ===== JWT =====
        services.AddApiAuthorization(configuration);

        // ===== CORS =====
        services.AddApiCors(configuration);

        // ===== Swagger =====
        services.AddApiSwagger();

        // ===== Repositories =====
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        // ===== Services =====
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoomTypeService, RoomTypeService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IRoomStatusUpdateService, RoomStatusUpdateService>();
        services.AddScoped<IBookingStatusUpdateService, BookingStatusUpdateService>();

        // ===== Background Services =====
        services.AddHostedService<RoomStatusUpdateBackgroundService>();
        services.AddHostedService<BookingStatusUpdateBackgroundService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Scoped);

        return services;
    }

    public static IServiceCollection AddApiAuthorization(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = jwtSettings["Key"];

        if (string.IsNullOrEmpty(key))
        {
            throw new Exception("JWT Key is missing in appsettings.json");
        }

        services
            .AddAuthentication(options =>
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        var response = context.Response;
                        response.StatusCode = StatusCodes.Status401Unauthorized;
                        response.ContentType = "text/plain; charset=utf-8";
                        await response.WriteAsync(
                            "Не авторизован. Пожалуйста, передайте правильный JWT токен"
                        );
                    },
                };
            });

        return services;
    }

    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Берём разрешённые origins из appsettings.json
        var allowedOrigins = configuration
            .GetSection("App:CorsOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("cors-policy", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    policy.SetIsOriginAllowed(_ => true);
                }

                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            });
        });

        return services;
    }

    private static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            var currentAssembly = Assembly.GetExecutingAssembly();

            var xmlDocs = currentAssembly
                .GetReferencedAssemblies()
                .Union([currentAssembly.GetName()])
                .Select(a => Path.Combine(AppContext.BaseDirectory, $"{a.Name}.xml"))
                .Where(f => File.Exists(f))
                .ToList();

            xmlDocs.ForEach(xmlDoc => options.IncludeXmlComments(xmlDoc));

            options.AddSecurityDefinition(
                JwtBearerDefaults.AuthenticationScheme,
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "Введите JWT токен авторизации.",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                }
            );

            options.OperationFilter<JwtAuthorizeFilter>();
        });

        return services;
    }

    public static WebApplication UseApiServices(
        this WebApplication app,
        IWebHostEnvironment webHostEnvironment
    )
    {
        app.UseExceptionHandler(options => { });

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseCors("cors-policy");

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseApiSwagger(webHostEnvironment);

        app.MapControllers();

        return app;
    }

    private static WebApplication UseApiSwagger(
        this WebApplication app,
        IWebHostEnvironment webHostEnvironment
    )
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