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

using ResortBooking.API;
using ResortBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Регистрируем все сервисы приложения
builder.Services
    .AddApiServices(builder.Configuration)           // API сервисы (контроллеры, swagger и т.д.)
    .AddOptions(builder.Configuration, builder.Environment)  // Конфигурация (опции для сервисов)
    .AddApiCors()                                   // CORS политика (разрешение кроссдоменных запросов)
    .AddInfrastructureServices(builder.Configuration);  // Infrastructure сервисы (БД, Repository, Services)
    //.AddApplicationServices()

var app = builder.Build();

app.UseApiServices(app.Environment);

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    response.ContentType = "text/plain; charset=utf-8";

    if (response.StatusCode == StatusCodes.Status415UnsupportedMediaType)
    {
        await response.WriteAsync("Неподдерживаемый тип содержимого. Ожидается 'application/json'.");
    }
    else if (response.StatusCode == StatusCodes.Status401Unauthorized)
    {
        await response.WriteAsync("Доступ запрещён. Требуется аутентификация.");
    }
});

app.Run();
