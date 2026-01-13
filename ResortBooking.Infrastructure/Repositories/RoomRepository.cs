using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;
using ResortBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _context;

    public RoomRepository(ApplicationDbContext context)
    {
        _context = context;
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
