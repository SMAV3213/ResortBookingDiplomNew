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

/// <summary>
/// Инъекция зависимостей (Dependency Injection) - IoC контейнер приложения.
///
/// Основная идея: вместо того, чтобы классы сами создавали свои зависимости,
/// мы передаём им зависимости извне. Это называется "dependency inversion".
///
/// Преимущества:
/// - 🔄 Легко менять реализацию (например, БД)
/// - ✅ Легче тестировать (можно внедрить mock объекты)
/// - 🧹 Чище код, меньше боилерплейта
///
/// Жизненные циклы регистрации:
/// - Transient: каждый раз новый экземпляр (не кешируется)
/// - Scoped: один экземпляр на один HTTP request (как сессия)
/// - Singleton: один экземпляр на всё приложение (меняется редко)
///
/// Пример регистрации:
///   services.AddScoped<IUserService, UserService>();
///   // Когда контроллер просит IUserService, контейнер создаст UserService
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Главный метод регистрации всех сервисов приложения.
    /// Вызывается из Program.cs: builder.Services.AddApiServices();
    ///
    /// Здесь мы регистрируем:
    /// - DbContext (подключение к БД)
    /// - Репозитории (доступ к данным)
    /// - Бизнес-логика сервисы
    /// - FluentValidation (проверка данных)
    /// - Background services (автоматические задачи)
    /// - Swagger (документация)
    /// - JWT аутентификация
    /// </summary>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Добавляем документацию API (Swagger/OpenAPI)
        services
            .AddEndpointsApiExplorer()
            // Добавляем контроллеры
            .AddControllers()
            // Настраиваем JSON сериализацию - конвертируем enum в string
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // Подключаем Entity Framework с SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
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

        services.AddScoped<IAuthService, AuthService>(); // Аутентификация
        services.AddScoped<IRoomTypeService, RoomTypeService>(); // Типы номеров
        services.AddScoped<IRoomService, RoomService>(); // Номера
        services.AddScoped<IBookingService, BookingService>(); // Бронирования
        services.AddScoped<IUserService, UserService>(); // Пользователи

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

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Scoped);

        return services;
    }

    /// <summary>
    /// Настраивает JWT аутентификацию. Это сердце безопасности приложения!
    ///
    /// Как работает JWT (JSON Web Token):
    /// 1. Пользователь логинится → сервер создает токен
    /// 2. Токен содержит: ID пользователя, роль, время жизни
    /// 3. Токен подписывается секретным ключом (в appsettings.json)
    /// 4. Клиент отправляет токен в заголовке Authorization: Bearer {token}
    /// 5. Сервер проверяет подпись и разрешает/запрещает доступ
    ///
    /// Безопасность:
    /// - Токен нельзя подделать без секретного ключа
    /// - Токен имеет срок действия (15 минут по умолчанию)
    /// - При истечении клиент должен обновить токен
    ///
    /// appsettings.json должен содержать:
    /// "Jwt": {
    ///   "SecretKey": "very-long-secret-key-min-32-chars",
    ///   "Issuer": "ResortBooking",
    ///   "Audience": "ResortBookingClients",
    ///   "ExpirationMinutes": 15
    /// }
    /// </summary>
    public static IServiceCollection AddApiAuthorization(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        using var provider = services.BuildServiceProvider();
        var jwtSettings = configuration.GetSection("Jwt");

        // Регистрируем JWT Bearer аутентификацию
        services
            .AddAuthentication(options =>
            {
                // Говорим приложению: используй JWT для аутентификации
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // Если токен неверный, отправь 401 Unauthorized
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Параметры для проверки и валидации токена
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Проверяем наличие и корректность всех параметров:

                    // Кто выдал токен? (защита от токенов с других сервисов)
                    ValidateIssuer = true,

                    // Для кого этот токен? (дополнительная проверка назначения)
                    ValidateAudience = true,

                    // Не истёк ли срок действия? (токены "живут" ограниченное время)
                    ValidateLifetime = true,

                    // Правильная подпись? (никто не подделал токен)
                    ValidateIssuerSigningKey = true,

                    // Допустимые значения, которые мы проверяем выше
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],

                    // Секретный ключ для проверки подписи
                    // ВАЖНО: должен совпадать с ключом, которым создавался токен!
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
                    ),
                };

                // Обработчик ошибок при неверной аутентификации
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Обрабатываем challenge (401) событие самостоятельно
                        context.HandleResponse();

                        // Возвращаем понятное сообщение об ошибке
                        var response = context.Response;
                        response.StatusCode = StatusCodes.Status401Unauthorized;
                        response.ContentType = "text/plain; charset=utf-8";
                        var message = "Не авторизован. Пожалуйста, передайте правильный JWT токен";
                        await response.WriteAsync(message);
                    },
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

            var xmlDocs = currentAssembly
                .GetReferencedAssemblies()
                .Union([currentAssembly.GetName()])
                .Select(a => Path.Combine(AppContext.BaseDirectory, $"{a.Name}.xml"))
                .Where(f => File.Exists(f))
                .ToList();

            xmlDocs.ForEach(xmlDoc => options.IncludeXmlComments(xmlDoc));

            // Добавляем возможность передачи JWT токена
            options.AddSecurityDefinition(
                JwtBearerDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    Description = "Введите JWT токен авторизации.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                }
            );

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
    public static IServiceCollection AddApiCors(this IServiceCollection services)
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
                        .WithOrigins(app.CorsOrigins.ToArray()) // Разрешаем запросы с конкретных доменов
                        .AllowAnyHeader() // Разрешаем любые заголовки
                        .AllowAnyMethod() // Разрешаем любые HTTP методы
                        .AllowCredentials() // Разрешаем отправку credentials (cookies, auth headers)
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
    public static WebApplication UseApiServices(
        this WebApplication app,
        IWebHostEnvironment webHostEnvironment
    )
    {
        // Обработчик исключений
        app.UseExceptionHandler(options => { });

        app.UseAuth();
        // Регистрируем маршруты контроллеров
        app.MapControllers();

        app.UseHttpsRedirection();
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
