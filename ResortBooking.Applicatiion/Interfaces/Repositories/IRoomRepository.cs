using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.Interfaces.Repositories;

public interface IRoomRepository
{
    Task<List<Room>> GetAllAsync();
    Task<Room?> GetByIdAsync(Guid id);

    Task AddAsync(Room room);
    void Delete(Room room);
    void Update(Room room);
    Task<List<Room>> GetAvailableByRoomTypeAsync(Guid roomTypeId);

    Task SaveChangesAsync();
}
