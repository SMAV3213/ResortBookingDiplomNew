namespace ResortBooking.Application.Interfaces.Services;

public interface IBookingStatusUpdateService
{
    Task UpdateBookingStatusesAsync(CancellationToken cancellationToken = default);
}
