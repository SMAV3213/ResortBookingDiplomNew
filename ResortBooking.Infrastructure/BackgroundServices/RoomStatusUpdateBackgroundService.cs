using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResortBooking.Application.Interfaces.Services;

namespace ResortBooking.Infrastructure.BackgroundServices;

public class RoomStatusUpdateBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RoomStatusUpdateBackgroundService> _logger;
    private Timer? _timer;

    public RoomStatusUpdateBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RoomStatusUpdateBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Сервис обновления статусов комнат запущен");

        // Выполняем обновление при запуске приложения
        await ExecuteUpdateAsync(cancellationToken);

        // Планируем запуск каждый день в 00:00
        ScheduleNextRun();

        await base.StartAsync(cancellationToken);
    }

    private void ScheduleNextRun()
    {
        var now = DateTime.UtcNow;
        // Берём сегодняшнюю дату и добавляем 1 день, получаем завтрашний день в 00:00:00 UTC
        var nextRunTime = now.Date.AddDays(1);

        // Считаем сколько времени осталось до следующего запуска
        var delay = nextRunTime - now;

        // Запускаем таймер с рассчитанной задержкой, затем повторяем каждый день
        _timer = new Timer(async _ => await TimerCallbackAsync(), null, delay, TimeSpan.FromDays(1));

        _logger.LogInformation("Следующее обновление статусов запланировано на {NextRunTime} UTC", nextRunTime);
    }

    private async Task TimerCallbackAsync()
    {
        try
        {
            await ExecuteUpdateAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статусов комнат");
        }
    }

    private async Task ExecuteUpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IRoomStatusUpdateService>();

            _logger.LogInformation("Начало обновления статусов комнат");
            await service.UpdateRoomStatusesAsync(cancellationToken);
            _logger.LogInformation("Статусы комнат успешно обновлены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении обновления статусов комнат");
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
        _logger.LogInformation("Сервис обновления статусов комнат остановлен");
        _timer?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
