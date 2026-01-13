using Microsoft.AspNetCore.Http;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.Application.Interfaces.Services;

public interface IRoomTypeService
{
    Task<ApiResponse<List<RoomTypeDTO>>> GetAllAsync();
    Task<ApiResponse<RoomTypeDTO>> GetByIdAsync(Guid id);
    Task<List<RoomType>> GetAvailableRoomTypesAsync(int guests , DateTime checkIn, DateTime checkOut);

    Task<ApiResponse<Guid>> CreateAsync(CreateRoomTypeDTO dto, List<IFormFile> images);
    Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateRoomTypeDTO dto, List<IFormFile>? images);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
