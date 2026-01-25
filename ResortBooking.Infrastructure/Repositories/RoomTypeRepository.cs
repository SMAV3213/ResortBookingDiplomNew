using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces.Repositories;
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
