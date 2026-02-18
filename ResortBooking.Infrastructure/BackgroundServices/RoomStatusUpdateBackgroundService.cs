using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResortBooking.Application.Interfaces.Services;

namespace ResortBooking.Infrastructure.BackgroundServices;

/// <summary>
/// Background Service для автоматического обновления статусов комнат.
/// 
/// Что он делает:
/// 1. При запуске приложения сразу проверяет все комнаты
/// 2. Каждый день в 00:00 UTC проверяет занятость комнат
/// 3. Обновляет статус комнаты в зависимости от активных бронирований
///    (Occupied если есть гость, Available если свободна, Maintenance и т.д.)
/// 
/// Зачем это нужно:
/// - Синхронизирует состояние комнаты с реальностью
/// - Клиенты видят актуальный статус комнаты
/// - Предотвращает двойное бронирование
/// 
/// ПРИМЕЧАНИЕ: Если у вас несколько серверов, это может работать дважды.
/// Для продакшена используйте Hangfire или других scheduler.
/// </summary>
public class RoomStatusUpdateBackgroundService : BackgroundService
{
    // IServiceProvider нужен для создания scope при каждом запуске
    private readonly IServiceProvider _serviceProvider;

    // Логирование для мониторинга работы сервиса
    private readonly ILogger<RoomStatusUpdateBackgroundService> _logger;

    // Таймер для периодического запуска
    private Timer? _timer;

    public RoomStatusUpdateBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RoomStatusUpdateBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Вызывается при запуске приложения.
    /// 1. Сразу выполняет первую проверку статусов комнат
    /// 2. Планирует повторяющиеся проверки на каждый день в 00:00
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Сервис обновления статусов комнат запущен");

        // Выполняем обновление при запуске приложения
        // Это гарантирует, что после перезагрузки статусы актуальны
        await ExecuteUpdateAsync(cancellationToken);

        // Планируем запуск каждый день в 00:00 UTC
        ScheduleNextRun();

        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Вычисляет время следующего запуска и создает таймер.
    /// 
    /// Пример:
    /// - Сейчас: 2024-12-15 14:30:00 UTC
    /// - Следующий запуск: 2024-12-16 00:00:00 UTC
    /// - Задержка: 9 часов 30 минут
    /// - После первого запуска: повторяется каждые 24 часа
    /// </summary>
    private void ScheduleNextRun()
    {
        var now = DateTime.UtcNow;

        // Берём текущую дату без времени (00:00:00), потом добавляем 1 день
        // Результат: завтра в 00:00:00 UTC
        var nextRunTime = now.Date.AddDays(1);

        // Считаем сколько осталось времени до следующего запуска
        // Если сейчас 14:30, а запуск в 00:00, то delay = 9.5 часов
        var delay = nextRunTime - now;

        // Создаём таймер:
        // - Первый запуск через 'delay' времени (завтра в 00:00)
        // - Затем повторяется каждые TimeSpan.FromDays(1) = 24 часа
        _timer = new Timer(async _ => await TimerCallbackAsync(), null, delay, TimeSpan.FromDays(1));

        _logger.LogInformation("Следующее обновление статусов запланировано на {NextRunTime} UTC", nextRunTime);
    }

    /// <summary>
    /// Обработчик срабатывания таймера.
    /// Мы оборачиваем в try-catch, чтобы если возникнет ошибка,
    /// таймер не сломался и продолжал работать.
    /// </summary>
    private async Task TimerCallbackAsync()
    {
        try
        {
            await ExecuteUpdateAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не выбрасываем исключение
            // Иначе таймер остановится и больше не будет запускаться
            _logger.LogError(ex, "Ошибка при обновлении статусов комнат");
        }
    }

    /// <summary>
    /// Основная логика обновления статусов комнат.
    /// 
    /// Процесс:
    /// 1. Создаём DI scope (область видимости для Scoped сервисов)
    /// 2. Извлекаем сервис обновления статусов
    /// 3. Вызываем метод обновления
    /// 4. Логируем результат
    /// </summary>
    private async Task ExecuteUpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Создаём новый scope - это как отдельная сессия
            // Для каждого запуска нужен свой scope, потому что:
            // - DbContext должен быть fresh (чистый)
            // - Предотвращаем утечки памяти
            // - Каждый scope имеет свой набор Scoped сервисов
            using var scope = _serviceProvider.CreateScope();

            // Извлекаем сервис из контейнера
            // GetRequiredService выбросит исключение если сервис не зарегистрирован
            var service = scope.ServiceProvider.GetRequiredService<IRoomStatusUpdateService>();

            _logger.LogInformation("Начало обновления статусов комнат");

            // Вызываем основной метод
            // Этот метод проверяет все активные бронирования
            // и обновляет статус комнат соответственно
            await service.UpdateRoomStatusesAsync(cancellationToken);

            _logger.LogInformation("Статусы комнат успешно обновлены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении обновления статусов комнат");
        }
    }

    /// <summary>
    /// Требуемая реализация BackgroundService.
    /// 
    /// Это абстрактный метод, который ДОЛЖЕН быть переопределен.
    /// Но мы НЕ используем его в нашей реализации!
    /// 
    /// Вместо этого мы:
    /// - Переопределили StartAsync (запуск сервиса)
    /// - Используем таймер в ScheduleNextRun
    /// - Просто ждём сигнала о выключении приложения
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // BackgroundService требует реализации ExecuteAsync
        // но вся логика уже обработана в ScheduleNextRun и таймере

        // Ждём сигнала стопа от приложения (stoppingToken)
        // Timeout.Infinite = ждём бесконечно до отмены
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    /// Вызывается при выключении приложения.
    /// Здесь мы чистим ресурсы (особенно таймер).
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Сервис обновления статусов комнат остановлен");

        // Освобождаем ресурсы таймера
        // Это важно для избежания утечек памяти
        _timer?.Dispose();

        await base.StopAsync(cancellationToken);
    }
}
