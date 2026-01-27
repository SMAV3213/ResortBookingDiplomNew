using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;
using ResortBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.Infrastructure.Repositories;

public class RoomTypeRepository : IRoomTypeRepository
{
    private readonly ApplicationDbContext _context;

    public RoomTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<RoomTypeWithoutRoomsDTO>> SearchAsync(RoomTypesQueryDTO q, CancellationToken ct = default)
    {
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => q.PageSize
        };

        var query = _context.RoomTypes
            .AsNoTracking()
            .Include(rt => rt.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            query = query.Where(rt => rt.Name.Contains(s) || rt.Description.Contains(s));
        }

        if (q.MinCapacity.HasValue) query = query.Where(rt => rt.Capacity >= q.MinCapacity.Value);
        if (q.MaxCapacity.HasValue) query = query.Where(rt => rt.Capacity <= q.MaxCapacity.Value);

        if (q.MinPrice.HasValue) query = query.Where(rt => rt.PricePerNight >= q.MinPrice.Value);
        if (q.MaxPrice.HasValue) query = query.Where(rt => rt.PricePerNight <= q.MaxPrice.Value);

        var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = (q.SortBy?.ToLowerInvariant(), desc) switch
        {
            ("price", true) => query.OrderByDescending(x => x.PricePerNight).ThenByDescending(x => x.Id),
            ("price", false) => query.OrderBy(x => x.PricePerNight).ThenBy(x => x.Id),

            ("capacity", true) => query.OrderByDescending(x => x.Capacity).ThenByDescending(x => x.Id),
            ("capacity", false) => query.OrderBy(x => x.Capacity).ThenBy(x => x.Id),

            ("name", true) => query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Name).ThenBy(x => x.Id),
        };

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(rt => new RoomTypeWithoutRoomsDTO(
                rt.Id,
                rt.Name,
                rt.Description,
                rt.Capacity,
                rt.PricePerNight,
                rt.Images.Select(i => i.FilePath).ToList()
            ))
            .ToListAsync(ct);

        return new PagedResult<RoomTypeWithoutRoomsDTO>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<List<RoomType>> GetAllAsync()
        => _context.RoomTypes
            .Include(x => x.Rooms)
            .Include(x => x.Images)
            .ToListAsync();

    public Task<RoomType?> GetByIdAsync(Guid id)
        => _context.RoomTypes
            .Include(x => x.Rooms)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(RoomTypeWithoutRoomsDTO roomType)
    {
        var entity = new RoomType
        {
            Id = roomType.Id,
            Name = roomType.Name,
            Description = roomType.Description,
            Capacity = roomType.Capacity,
            PricePerNight = roomType.PricePerNight,
            Images = roomType.ImageUrls != null
                ? roomType.ImageUrls.Select(url => new RoomTypeImage { FilePath = url }).ToList()
                : new List<RoomTypeImage>()
        };
        await _context.RoomTypes.AddAsync(entity);
    }

    public void Update(RoomType roomType)
        => _context.RoomTypes.Update(roomType);

    public void Remove(RoomType roomType)
        => _context.RoomTypes.Remove(roomType);

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();

    public async Task<List<RoomTypeWithoutRoomsDTO>> GetAvailableRoomTypesAsync(int guests, DateTime checkIn, DateTime checkOut)
    {
        var roomTypes = await _context.RoomTypes
            .Include(rt => rt.Images)
            .Include(rt => rt.Rooms)
            .ThenInclude(r => r.Bookings)
            .ToListAsync();

        var availableTypes = roomTypes
            .Where(rt =>
                rt.Capacity >= guests &&
                rt.Rooms.Any(r =>
                    r.Bookings
                     .Where(b => b.Status != BookingStatus.Cancelled)
                     .All(b =>
                        checkOut <= b.CheckInDate || checkIn >= b.CheckOutDate
                     )
                )
            )
            .Select(rt => new RoomTypeWithoutRoomsDTO(
                rt.Id,
                rt.Name,
                rt.Description,
                rt.Capacity,
                rt.PricePerNight,
                rt.Images?.Select(img => img.FilePath).ToList() ?? new List<string>()
            ))
            .ToList();

        return availableTypes;
    }
}
