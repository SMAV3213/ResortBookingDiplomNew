using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResortBooking.Application.Interfaces.Services;

namespace ResortBooking.Infrastructure.BackgroundServices;

/// <summary>
/// Background Service для автоматического обновления статусов бронирований.
/// 
/// Что он делает:
/// 1. При запуске приложения сразу проверяет все бронирования
/// 2. Каждый день в 00:00 UTC запускается повторная проверка
/// 3. Обновляет статусы: выполненные → Completed, неподтвержденные → отменяются
/// 
/// Почему Background Service нужен?
/// - Асинхронное выполнение (не блокирует основной поток приложения)
/// - Автоматическое управление жизненным циклом (.NET запускает при старте, останавливает при выключении)
/// - Поддержка логирования и обработки ошибок
/// 
/// ВАЖНО: Этот сервис работает только на одном сервере!
/// Если у вас несколько инстансов приложения, задача может запуститься дважды.
/// Решение: используйте Hangfire или Azure Scheduler для продакшена.
/// </summary>
public class BookingStatusUpdateBackgroundService : BackgroundService
{
    // IServiceProvider нужен, чтобы создавать scope для каждого запуска
    // Это необходимо для работы с Scoped сервисами (DbContext, репозитории и т.д.)
    private readonly IServiceProvider _serviceProvider;

    // Логирование для отслеживания работы сервиса
    private readonly ILogger<BookingStatusUpdateBackgroundService> _logger;

    // Таймер для периодического запуска задачи
    private Timer? _timer;

    public BookingStatusUpdateBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BookingStatusUpdateBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Вызывается при запуске приложения.
    /// Здесь мы:
    /// 1. Сразу выполняем первую проверку
    /// 2. Планируем периодические запуски на каждые 24 часа
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Сервис обновления статусов бронирований запущен");

        // Выполняем обновление при запуске приложения
        // Это гарантирует, что старые бронирования обновятся даже если приложение перезагружалось
        await ExecuteUpdateAsync(cancellationToken);

        // Планируем запуск каждый день в 00:00 UTC
        ScheduleNextRun();

        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Вычисляет время следующего запуска и создает таймер.
    /// 
    /// Логика:
    /// - Текущее время: 2024-12-15 14:30:00
    /// - Следующий запуск: 2024-12-16 00:00:00 (завтра в полночь)
    /// - Delay: 9.5 часов
    /// - После первого запуска: повторяется каждые 24 часа
    /// </summary>
    private void ScheduleNextRun()
    {
        var now = DateTime.UtcNow;

        // Вычисляем время следующего запуска: завтра в 00:00:00 UTC
        // Date дает дату без времени (00:00:00), AddDays(1) дает завтра
        var nextRunTime = now.Date.AddDays(1);

        // Задержка до следующего запуска
        // Если now = 14:30, а nextRunTime = завтра 00:00
        // то delay = 9 часов 30 минут
        var delay = nextRunTime - now;

        // Создаём периодический таймер:
        // - Первый запуск через 'delay' (завтра в 00:00)
        // - Затем повторяется каждые 24 часа (Period = TimeSpan.FromDays(1))
        // 
        // Параметры Timer:
        // - Callback: асинхронный метод TimerCallbackAsync
        // - state: null (не используется)
        // - dueTime: delay (когда первый раз запустить)
        // - period: 24 часа (как часто повторять)
        _timer = new Timer(async _ => await TimerCallbackAsync(), null, delay, TimeSpan.FromDays(1));

        _logger.LogInformation("Следующее обновление статусов бронирований запланировано на {NextRunTime} UTC", nextRunTime);
    }

    /// <summary>
    /// Обработчик срабатывания таймера.
    /// Оборачиваем вызов в try-catch, чтобы сервис не упал при ошибке.
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
            _logger.LogError(ex, "Ошибка при обновлении статусов бронирований");
        }
    }

    /// <summary>
    /// Основная логика обновления статусов бронирований.
    /// 
    /// Шаги:
    /// 1. Создаём новый DI scope (каждый запуск должен иметь свой scope)
    /// 2. Получаём сервис обновления статусов из контейнера
    /// 3. Вызываем метод обновления
    /// 4. Логируем результат
    /// </summary>
    private async Task ExecuteUpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Создаём новый scope для Scoped сервисов (DbContext, репозитории)
            // Scope = область видимости, которая существует на время одного запроса
            // После using блока все Scoped сервисы будут очищены (освобождение ресурсов)
            using var scope = _serviceProvider.CreateScope();

            // Получаём сервис из контейнера DI
            var service = scope.ServiceProvider.GetRequiredService<IBookingStatusUpdateService>();

            _logger.LogInformation("Начало обновления статусов бронирований");

            // Вызываем основной метод обновления
            // Этот метод проверяет каждое бронирование и обновляет его статус
            await service.UpdateBookingStatusesAsync(cancellationToken);

            _logger.LogInformation("Статусы бронирований успешно обновлены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении обновления статусов бронирований");
        }
    }

    /// <summary>
    /// Требуемая реализация BackgroundService.
    /// 
    /// ВАЖНО: Мы НЕ используем этот метод!
    /// Вместо этого мы переопределили StartAsync и используем таймер.
    /// 
    /// Просто ждём сигнала остановки приложения (Task.Delay(Timeout.Infinite)).
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // BackgroundService требует реализации ExecuteAsync
        // но вся логика уже обработана в ScheduleNextRun и таймере

        // Ждём сигнала остановки (cancellation token)
        // Timeout.Infinite значит "жди бесконечно" 
        // stoppingToken будет сигнализировать об остановке при выключении приложения
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    /// Вызывается при остановке приложения.
    /// Здесь мы очищаем ресурсы (таймер).
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Сервис обновления статусов бронирований остановлен");

        // Очищаем таймер (освобождаем ресурсы)
        _timer?.Dispose();

        await base.StopAsync(cancellationToken);
    }
}
