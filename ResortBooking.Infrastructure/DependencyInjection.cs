using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResortBooking.Application.Data.Options;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Infrastructure.Data.Options;
using ResortBooking.Infrastructure.Persistence;
using ResortBooking.Infrastructure.Services;

namespace ResortBooking.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Добавления Infrastructure сервисов.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    /// <summary>
    /// Добавления опций из конфигураций.
    /// </summary>
    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // Вместо ручного создания экземпляра и AddSingleton, используем встроенный механизм
        // Это решит проблему с 'implementationInstance' is null
        var appOptionsSection = configuration.GetSection("App");
        
        // Регистрация через IOptions<AppOptions>
        services.Configure<AppOptions>(appOptionsSection);
        
        // Если вам ОЧЕНЬ нужно регистрировать именно интерфейс IAppOptions напрямую:
        var appOptions = appOptionsSection.Get<AppOptions>();
        if (appOptions == null)
        {
            // Создаем пустой объект, чтобы API не падало, если конфиг забыли
            appOptions = new AppOptions { CorsOrigins = new List<string>() };
        }
        services.AddSingleton<IAppOptions>(appOptions);

        return services;
    }
    }
