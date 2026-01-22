using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ResortBooking.Application.Data.Options;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Infrastructure.Data.Options;
using ResortBooking.Infrastructure.Persistence;
using ResortBooking.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Добавления Infrastructure сервисов.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices
          (this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    /// <summary>
    /// Добавления опций из конфигураций.
    /// </summary>
    public static IServiceCollection AddOptions(this IServiceCollection services,
        IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        var appOptions = configuration.GetSection("App").Get<AppOptions>()!;
        //var jwtOptions = configuration.GetSection("JWT").Get<JwtOptions>()!;

        services.AddSingleton<IAppOptions>(appOptions);

        return services;
    }

}
