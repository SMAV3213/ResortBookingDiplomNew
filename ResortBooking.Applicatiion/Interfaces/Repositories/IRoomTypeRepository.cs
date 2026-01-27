using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.Application.Interfaces.Repositories;

public interface IRoomTypeRepository
{
    Task<PagedResult<RoomTypeWithoutRoomsDTO>> SearchAsync(RoomTypesQueryDTO query, CancellationToken ct = default);
    Task<List<RoomType>> GetAllAsync();
    Task<List<RoomTypeWithoutRoomsDTO>> GetAvailableRoomTypesAsync(int guests, DateTime checkIn, DateTime checkOut);
    Task<RoomType?> GetByIdAsync(Guid id);

    Task AddAsync(RoomTypeWithoutRoomsDTO roomType);
    void Update(RoomType roomType);
    void Remove(RoomType roomType);

    Task SaveChangesAsync();
}
