using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces;
using ResortBooking.Domain.Entities;
using ResortBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Repositories;

public class RoomTypeRepository : IRoomTypeRepository
{
    private readonly ResortBookingDbContext _db;

    public RoomTypeRepository(ResortBookingDbContext db)
    {
        _db = db;
    }

    public async Task<List<RoomType>> GetAllAsync()
    {
        return await _db.RoomTypes.Include(rt => rt.Images).ToListAsync();
    }

    public async Task<RoomType?> GetByIdAsync(Guid id)
    {
        return await _db.RoomTypes.Include(rt => rt.Images)
                                  .FirstOrDefaultAsync(rt => rt.Id == id);
    }

    public async Task AddAsync(RoomType roomType)
    {
        _db.RoomTypes.Add(roomType);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(RoomType roomType)
    {
        _db.RoomTypes.Update(roomType);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(RoomType roomType)
    {
        _db.RoomTypes.Remove(roomType);
        await _db.SaveChangesAsync();
    }
}
