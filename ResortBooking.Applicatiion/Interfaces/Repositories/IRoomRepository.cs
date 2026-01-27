using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomDTOs;

namespace ResortBooking.Application.Interfaces.Repositories;

public interface IRoomRepository
{
    Task<PagedResult<Room>> SearchAsync(RoomsQueryDTO query, CancellationToken ct = default);
    Task<List<Room>> GetAllAsync();
    Task<Room?> GetByIdAsync(Guid id);

    Task AddAsync(Room room);
    void Delete(Room room);
    void Update(Room room);
    Task<List<Room>> GetAvailableByRoomTypeAsync(Guid roomTypeId);

    Task SaveChangesAsync();
}
