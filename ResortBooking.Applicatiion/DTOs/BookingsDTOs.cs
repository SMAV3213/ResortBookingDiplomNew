using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.DTOs;

public class BookingsDTOs
{
    public record BookingDTO(
    Guid Id,
    Guid RoomId,
    Guid UserId,
    DateTime CheckIn,
    DateTime CheckOut,
    int GuestsCount,
    decimal TotalPrice,
    string Status
);

public record CreateBookingDTO(
    Guid UserId,
    Guid RoomTypeId,
    DateTime CheckIn,
    DateTime CheckOut,
    int GuestsCount
);

public record UpdateBookingDTO(
    DateTime CheckIn,
    DateTime CheckOut,
    int GuestsCount
);
}
