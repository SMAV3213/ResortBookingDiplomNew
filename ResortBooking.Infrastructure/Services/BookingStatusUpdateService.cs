using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;

namespace ResortBooking.Infrastructure.Services;

public class BookingStatusUpdateService : IBookingStatusUpdateService
{
    private readonly IBookingRepository _bookingRepository;

    public BookingStatusUpdateService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task UpdateBookingStatusesAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        // Получаем все бронирования из БД
        var bookings = await _bookingRepository.GetAllAsync();

        // Фильтруем только активные бронирования (не отменённые и не завершённые)
        // Created и Confirmed - это статусы, которые могут измениться на Completed
        var activeBookings = bookings
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Created)
            .ToList();

        var bookingsToUpdate = new List<Booking>();

        // Проходим по каждому активному бронированию
        foreach (var booking in activeBookings)
        {
            // Проверяем: прошла ли уже дата выезда (checkout)?
            // Если CheckOutDate < сегодня, значит гость уже выехал - отмечаем бронь как завершённую
            if (booking.CheckOutDate.Date < today)
            {
                // Дополнительная проверка: может быть другой process уже обновил статус
                if (booking.Status != BookingStatus.Completed)
                {
                    booking.Status = BookingStatus.Completed;
                    bookingsToUpdate.Add(booking);
                }
            }
        }

        // Обновляем все изменённые бронирования в базе
        foreach (var booking in bookingsToUpdate)
        {
            _bookingRepository.Update(booking);
        }

        // Сохраняем только если были какие-то изменения (экономим запросы в БД)
        if (bookingsToUpdate.Count > 0)
        {
            await _bookingRepository.SaveChangesAsync();
        }
    }
}
