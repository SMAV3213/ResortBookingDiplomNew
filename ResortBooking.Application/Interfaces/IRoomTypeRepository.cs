using ResortBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.Interfaces;

public interface IRoomTypeRepository
{
    Task<List<RoomType>> GetAllAsync();
    Task<RoomType?> GetByIdAsync(Guid id);
    Task AddAsync(RoomType roomType);
    Task UpdateAsync(RoomType roomType);
    Task DeleteAsync(RoomType roomType);
}
