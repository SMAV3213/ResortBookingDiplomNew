using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Domain.Entites;
using ResortBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Booking>> GetAllAsync() =>
        await _context.Bookings.Include(b => b.Room).ToListAsync();

    public async Task<Booking?> GetByIdAsync(Guid id) =>
        await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == id);

    public async Task AddAsync(Booking booking) =>
        await _context.Bookings.AddAsync(booking);

    public void Update(Booking booking) =>
        _context.Bookings.Update(booking);

    public void Delete(Booking booking) =>
        _context.Bookings.Remove(booking);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public async Task<List<Booking>> GetByRoomIdAsync(Guid roomId) =>
        await _context.Bookings.Where(b => b.RoomId == roomId && b.Status == Domain.Enums.BookingStatus.Confirmed)
                               .ToListAsync();

    public async Task<List<Booking>> GetByUserIdAsync(Guid userId) =>
        await _context.Bookings.Where(b => b.UserId == userId).ToListAsync();

    public async Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime checkIn, DateTime checkOut) =>
        await _context.Bookings
            .Where(b => b.Status == Domain.Enums.BookingStatus.Confirmed &&
                        b.CheckInDate < checkOut && b.CheckOutDate > checkIn)
            .ToListAsync();
}
