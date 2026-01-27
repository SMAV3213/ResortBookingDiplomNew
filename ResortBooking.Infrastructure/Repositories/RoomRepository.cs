using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;
using ResortBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomDTOs;

namespace ResortBooking.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _context;

    public RoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Room>> SearchAsync(RoomsQueryDTO q, CancellationToken ct = default)
    {
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => q.PageSize
        };

        var query = _context.Rooms
            .AsNoTracking()
            .Include(x => x.RoomType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            query = query.Where(r => r.Number.Contains(s));
        }

        if (q.RoomTypeId.HasValue)
            query = query.Where(r => r.RoomTypeId == q.RoomTypeId.Value);

        if (!string.IsNullOrWhiteSpace(q.Status) &&
            Enum.TryParse<RoomStatus>(q.Status, ignoreCase: true, out var st))
        {
            query = query.Where(r => r.Status == st);
        }

        var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = (q.SortBy?.ToLowerInvariant(), desc) switch
        {
            ("status", true) => query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Id),
            ("status", false) => query.OrderBy(x => x.Status).ThenBy(x => x.Id),

            ("createdat", true) => query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id),
            ("createdat", false) => query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),

            ("number", true) => query.OrderByDescending(x => x.Number).ThenByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Number).ThenBy(x => x.Id),
        };

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Room>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<Room>> GetAllAsync() =>
        await _context.Rooms.Include(x => x.RoomType).ToListAsync();

    public async Task<Room?> GetByIdAsync(Guid id) =>
    await _context.Rooms.Include(x => x.RoomType).FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(Room room)
    {
         _context.Rooms.Add(room);
    }

    public void Update(Room room) =>
        _context.Rooms.Update(room);

    public void Delete(Room room) =>
        _context.Rooms.Remove(room);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public async Task<List<Room>> GetAvailableByRoomTypeAsync(Guid roomTypeId)
    {
        return await _context.Rooms
            .Where(r => r.RoomTypeId == roomTypeId && r.Status != RoomStatus.Maintenance)
            .ToListAsync();
    }
}
