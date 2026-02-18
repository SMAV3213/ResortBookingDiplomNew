using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Domain.Enums;

namespace ResortBooking.Infrastructure.Services;

public class RoomStatusUpdateService : IRoomStatusUpdateService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;

    public RoomStatusUpdateService(
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task UpdateRoomStatusesAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        // Получаем все комнаты из базы
        var rooms = await _roomRepository.GetAllAsync();

        // Ищем все подтвержденные бронирования, которые активны на сегодня
        // Условие: checkin <= сегодня И checkout > сегодня (значит гость уже заехал или заезжает сегодня, но ещё не выехал)
        var bookingsToday = await _bookingRepository.GetAllAsync();
        var activeBookings = bookingsToday
            .Where(b => b.Status == BookingStatus.Confirmed &&
                        b.CheckInDate.Date <= today &&
                        b.CheckOutDate.Date > today)
            .ToList();

        // Собираем IDs всех занятых комнат в HashSet для быстрого поиска (O(1) вместо O(n))
        var bookedRoomIds = activeBookings.Select(b => b.RoomId).ToHashSet();

        // Проходим по каждой комнате и определяем её статус
        foreach (var room in rooms)
        {
            // Пропускаем комнаты на техническом обслуживании - их статус не трогаем
            if (room.Status == RoomStatus.Maintenance)
                continue;

            // Если комната в списке забронированных - Occupied, иначе Available
            var newStatus = bookedRoomIds.Contains(room.Id) 
                ? RoomStatus.Occupied 
                : RoomStatus.Available;

            // Обновляем только если статус изменился (избегаем лишних записей в БД)
            if (room.Status != newStatus)
            {
                room.Status = newStatus;
                _roomRepository.Update(room);
            }
        }

        // Сохраняем все изменения за одну транзакцию
        await _roomRepository.SaveChangesAsync();
    }
}
