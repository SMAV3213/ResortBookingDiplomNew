/// <summary>
/// Точка входа приложения Resort Booking
///
/// Поток инициализации:
/// 1. Создаём WebApplicationBuilder - конфигуратор приложения
/// 2. Регистрируем сервисы в IoC контейнер (Dependency Injection)
/// 3. Собираем WebApplication
/// 4. Настраиваем middleware (как обрабатываются HTTP запросы)
/// 5. Запускаем приложение на слушание портов
/// </summary>
using Microsoft.EntityFrameworkCore;
using ResortBooking.API;
using ResortBooking.Infrastructure;
using ResortBooking.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Регистрируем все сервисы приложения
builder
    .Services.AddApiServices(builder.Configuration) // API сервисы (контроллеры, swagger и т.д.)
    .AddOptions(builder.Configuration, builder.Environment) // Конфигурация (опции для сервисов)
    .AddInfrastructureServices(builder.Configuration); // Infrastructure сервисы (БД, Repository, Services)

//.AddApplicationServices()

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("✅ Миграции успешно применены.");
        await DbInitializer.InitializeAsync(context);
        Console.WriteLine("✅ База данных инициализирована.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации базы данных.");
    }
}

app.UseApiServices(app.Environment);

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    response.ContentType = "text/plain; charset=utf-8";

    if (response.StatusCode == StatusCodes.Status415UnsupportedMediaType)
    {
        await response.WriteAsync(
            "Неподдерживаемый тип содержимого. Ожидается 'application/json'."
        );
    }
    else if (response.StatusCode == StatusCodes.Status401Unauthorized)
    {
        await response.WriteAsync("Доступ запрещён. Требуется аутентификация.");
    }
});

app.Run();
