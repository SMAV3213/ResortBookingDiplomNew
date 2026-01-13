using Microsoft.AspNetCore.Http;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomDTOs;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.Application.Interfaces.Services;

public interface IRoomService
{
    Task<ApiResponse<List<RoomDTO>>> GetAllAsync();
    Task<ApiResponse<RoomDTO>> GetByIdAsync(Guid id);

    Task<ApiResponse<Guid>> AddAsync(CreateRoomDTO dto);
    Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateRoomDTO dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<List<RoomDTO>>> GetAvailableRoomsAsync(Guid roomTypeId, DateTime checkIn, DateTime checkOut);

}
