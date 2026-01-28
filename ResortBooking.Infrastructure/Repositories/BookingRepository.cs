using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;
using ResortBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.BookingsDTOs;

namespace ResortBooking.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Booking>> SearchAsync(BookingsQueryDTO q, CancellationToken ct = default)
    {
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => q.PageSize
        };

        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Room)
            .Include(b => b.User)
            .AsQueryable();

        if (q.UserId.HasValue)
            query = query.Where(b => b.UserId == q.UserId.Value);

        if (q.RoomId.HasValue)
            query = query.Where(b => b.RoomId == q.RoomId.Value);

        if (q.RoomTypeId.HasValue)
            query = query.Where(b => b.Room.RoomTypeId == q.RoomTypeId.Value);

        if (!string.IsNullOrWhiteSpace(q.Status) && Enum.TryParse<BookingStatus>(q.Status, true, out var st))
            query = query.Where(b => b.Status == st);

        if (q.From.HasValue)
            query = query.Where(b => b.CheckInDate >= q.From.Value);

        if (q.To.HasValue)
            query = query.Where(b => b.CheckOutDate <= q.To.Value);

        if (q.CheckInFrom.HasValue)
            query = query.Where(b => b.CheckInDate >= q.CheckInFrom.Value);

        if (q.CheckInTo.HasValue)
            query = query.Where(b => b.CheckInDate < q.CheckInTo.Value);

        if (q.CheckOutFrom.HasValue)
            query = query.Where(b => b.CheckOutDate >= q.CheckOutFrom.Value);

        if (q.CheckOutTo.HasValue)
            query = query.Where(b => b.CheckOutDate < q.CheckOutTo.Value);

        var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (q.SortBy?.ToLower(), desc) switch
        {
            ("createdat", true) => query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id),
            ("createdat", false) => query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),

            ("totalprice", true) => query.OrderByDescending(x => x.TotalPrice).ThenByDescending(x => x.Id),
            ("totalprice", false) => query.OrderBy(x => x.TotalPrice).ThenBy(x => x.Id),

            ("checkin", false) => query.OrderBy(x => x.CheckInDate).ThenBy(x => x.Id),
            _ => query.OrderByDescending(x => x.CheckInDate).ThenByDescending(x => x.Id),
        };

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Booking>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<Booking>> GetAllAsync() =>
        await _context.Bookings.Include(b => b.Room).Include(u => u.User).ToListAsync();

    public async Task<Booking?> GetByIdAsync(Guid id) =>
        await _context.Bookings.Include(b => b.Room).Include(u => u.User).FirstOrDefaultAsync(b => b.Id == id);

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
