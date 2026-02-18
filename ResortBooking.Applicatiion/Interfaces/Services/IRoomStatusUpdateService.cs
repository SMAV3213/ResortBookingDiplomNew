namespace ResortBooking.Application.Interfaces.Services;

public interface IRoomStatusUpdateService
{
    Task UpdateRoomStatusesAsync(CancellationToken cancellationToken = default);
}
