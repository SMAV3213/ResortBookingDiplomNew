using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.BookingsDTOs;

namespace ResortBooking.Application.Interfaces.Repositories;
public interface IBookingRepository
{
    Task<PagedResult<Booking>> SearchAsync(BookingsQueryDTO query, CancellationToken ct = default);
    Task<List<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(Guid id);

    Task AddAsync(Booking booking);
    void Update(Booking booking);
    void Delete(Booking booking);

    Task<List<Booking>> GetByRoomIdAsync(Guid roomId);
    Task<List<Booking>> GetByUserIdAsync(Guid userId);
    Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime checkIn, DateTime checkOut);
    Task SaveChangesAsync();
}
