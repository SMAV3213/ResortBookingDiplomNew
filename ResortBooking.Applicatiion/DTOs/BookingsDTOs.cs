using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.DTOs;

public class BookingsDTOs
{
    public record BookingDTO(
    Guid Id,
    Guid RoomId,
    string Number,
    Guid UserId,
    string login,
    DateTime CheckIn,
    DateTime CheckOut,
    int GuestsCount,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAt
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

public record BookingsQueryDTO(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,      
    Guid? UserId = null,       
    Guid? RoomId = null,
    Guid? RoomTypeId = null,
    DateTime? From = null,      
    DateTime? To = null,        
    string SortBy = "checkIn",  
    string SortDir = "desc" 
);
}
