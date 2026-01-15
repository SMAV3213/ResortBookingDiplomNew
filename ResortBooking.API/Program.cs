using ResortBooking.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices(builder.Configuration);
    //.AddApplicationServices()
    //.AddInfrastructureServices(builder.Configuration, builder.Environment)

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
