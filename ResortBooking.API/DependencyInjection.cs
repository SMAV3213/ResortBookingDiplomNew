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
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace ResortBooking.API;

/// <summary>
/// Инъекция зависимостей (Dependency Injection)
/// 
/// Здесь мы регистрируем все сервисы, репозитории и middleware
/// в IoC контейнер приложения.
/// 
/// Это позволяет ASP.NET автоматически внедрять зависимости
/// в конструкторы контроллеров, сервисов и т.д.
/// 
/// Жизненные циклы:
/// - Transient: новый экземпляр каждый раз
/// - Scoped: один экземпляр на HTTP request
/// - Singleton: один на всё приложение
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует все API сервисы:
    /// - Контроллеры и endpoints
    /// - Entity Framework DbContext
    /// - FluentValidation
    /// - Репозитории (Database access)
    /// - Business Logic сервисы
    /// - Background Services (автоматические задачи)
    /// </summary>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Добавляем документацию API (Swagger/OpenAPI)
        services
            .AddEndpointsApiExplorer()
            // Добавляем контроллеры
            .AddControllers()
            // Настраиваем JSON сериализацию - конвертируем enum в string
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter()
                );
            });

        // Подключаем Entity Framework с SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });

        // Добавляем FluentValidation - автоматическая валидация DTOs
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(typeof(UpdateUserDTOValidator).Assembly);

        // Кешируем данные в памяти приложения
        services.AddMemoryCache();

        // Настраиваем JWT аутентификацию
        services.AddApiAuthorization(configuration);

        // Настраиваем Swagger документацию
        services.AddApiSwagger();

        // ==================== РЕГИСТРИРУЕМ РЕПОЗИТОРИИ ====================
        // Scoped = новый экземпляр на каждый HTTP request
        // Это обеспечивает безопасность данных между запросами

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        // ==================== РЕГИСТРИРУЕМ БИЗНЕС-ЛОГИКУ ====================
        // Сервисы выполняют бизнес-правила (проверки, расчёты и т.д.)

        services.AddScoped<IAuthService, AuthService>();              // Аутентификация
        services.AddScoped<IRoomTypeService, RoomTypeService>();      // Типы номеров
        services.AddScoped<IRoomService, RoomService>();              // Номера
        services.AddScoped<IBookingService, BookingService>();        // Бронирования
        services.AddScoped<IUserService, UserService>();              // Пользователи

        // ==================== СЕРВИСЫ ОБНОВЛЕНИЯ СТАТУСОВ ====================
        // Эти сервисы содержат логику обновления статусов комнат и бронирований

        services.AddScoped<IRoomStatusUpdateService, RoomStatusUpdateService>();
        services.AddScoped<IBookingStatusUpdateService, BookingStatusUpdateService>();

        // ==================== BACKGROUND SERVICES ====================
        // Запускаются автоматически при старте приложения
        // Работают по расписанию (каждый день в 00:00 UTC)

        // Обновляет статусы комнат на основе активных бронирований
        services.AddHostedService<RoomStatusUpdateBackgroundService>();

        // Отмечает завершённые бронирования
        services.AddHostedService<BookingStatusUpdateBackgroundService>();

        // ==================== ВАЛИДАТОРЫ ====================
        // FluentValidation будет автоматически использовать эти валидаторы
        // при binding DTOs из HTTP запросов

        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly(),
            ServiceLifetime.Scoped
        );

        return services;
    }

    /// <summary>
    /// Настраивает JWT аутентификацию
    /// 
    /// JWT токен содержит информацию о пользователе и подписывается
    /// секретным ключом. Сервер проверяет подпись и роль пользователя.
    /// </summary>
    public static IServiceCollection AddApiAuthorization(
    this IServiceCollection services, IConfiguration configuration
    )
    {
        using var provider = services.BuildServiceProvider();
        var jwtSettings = configuration.GetSection("Jwt");

        // Добавляем JWT Bearer аутентификацию
        services.AddAuthentication(options =>
        {
            // По умолчанию используем JWT Bearer
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Настраиваем как проверять токен
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Проверяем все параметры
                ValidateIssuer = true,               // Кто выдал?
                ValidateAudience = true,             // Для кого?
                ValidateLifetime = true,             // Не истёк ли?
                ValidateIssuerSigningKey = true,     // Правильная подпись?

                // Допустимые значения
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
            };

            // Обработчик ошибок аутентификации
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

    /// <summary>
    /// Настраивает Swagger (API документация)
    /// 
    /// Swagger автоматически генерирует документацию из XML комментариев
    /// и позволяет тестировать API прямо из браузера
    /// </summary>
    private static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Загружаем XML документацию из сборок
            var currentAssembly = Assembly.GetExecutingAssembly();

            var xmlDocs = currentAssembly.GetReferencedAssemblies()
                .Union([currentAssembly.GetName()])
                .Select(a => Path.Combine(AppContext.BaseDirectory, $"{a.Name}.xml"))
                .Where(f => File.Exists(f)).ToList();

            xmlDocs.ForEach(xmlDoc => options.IncludeXmlComments(xmlDoc));

            // Добавляем возможность передачи JWT токена
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "Введите JWT токен авторизации.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });

            // Добавляем фильтр для авторизации в Swagger
            options.OperationFilter<JwtAuthorizeFilter>();
        });

        return services;
    }

    /// <summary>
    /// Добавляет CORS политику (разрешение запросов с других доменов)
    /// 
    /// Без CORS браузер не позволит JS коду с одного домена
    /// делать запросы на другой домен
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
                    // Разрешаем запросы с указанных доменов
                    policy
                        .WithOrigins([.. app.CorsOrigins])
                        .AllowAnyHeader()               // Разрешаем любые заголовки
                        .AllowAnyMethod()               // Разрешаем любые HTTP методы
                        .AllowCredentials()             // Разрешаем отправку credentials
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(3600))
                        .WithExposedHeaders("Content-Disposition");
                }
            );
        });

        return services;
    }

    /// <summary>
    /// Применяет middleware (обработчики HTTP запросов)
    /// 
    /// Middleware выполняется в порядке регистрации
    /// </summary>
    public static WebApplication UseApiServices(this WebApplication app,
    IWebHostEnvironment webHostEnvironment)
    {
        // Обработчик исключений
        app.UseExceptionHandler(options => { });

        // Регистрируем маршруты контроллеров
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
