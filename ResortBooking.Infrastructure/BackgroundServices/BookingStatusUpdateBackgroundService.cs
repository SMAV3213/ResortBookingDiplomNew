using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResortBooking.Application.Interfaces.Services;

namespace ResortBooking.Infrastructure.BackgroundServices;

public class BookingStatusUpdateBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingStatusUpdateBackgroundService> _logger;
    private Timer? _timer;

    public BookingStatusUpdateBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BookingStatusUpdateBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Сервис обновления статусов бронирований запущен");

        // Выполняем обновление при запуске приложения
        await ExecuteUpdateAsync(cancellationToken);

        // Планируем запуск каждый день в 00:00
        ScheduleNextRun();

        await base.StartAsync(cancellationToken);
    }

    private void ScheduleNextRun()
    {
        var now = DateTime.UtcNow;
        // Вычисляем время следующего запуска: завтра в 00:00:00
        var nextRunTime = now.Date.AddDays(1);

        // Задержка до следующего запуска
        var delay = nextRunTime - now;

        // Создаём периодический таймер: запускается через 'delay', потом повторяется каждые 24 часа
        _timer = new Timer(async _ => await TimerCallbackAsync(), null, delay, TimeSpan.FromDays(1));

        _logger.LogInformation("Следующее обновление статусов бронирований запланировано на {NextRunTime} UTC", nextRunTime);
    }

    private async Task TimerCallbackAsync()
    {
        try
        {
            await ExecuteUpdateAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статусов бронирований");
        }
    }

    private async Task ExecuteUpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBookingStatusUpdateService>();

            _logger.LogInformation("Начало обновления статусов бронирований");
            await service.UpdateBookingStatusesAsync(cancellationToken);
            _logger.LogInformation("Статусы бронирований успешно обновлены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении обновления статусов бронирований");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // BackgroundService требует реализации ExecuteAsync
        // но вся логика уже обработана в ScheduleNextRun
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Сервис обновления статусов бронирований остановлен");
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
