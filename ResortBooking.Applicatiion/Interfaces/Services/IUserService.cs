using ResortBooking.Application.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.UserDTOs;

namespace ResortBooking.Application.Interfaces.Services;

public interface IUserService
{
    Task<ApiResponse<List<UserDTO>>> GetAllAsync();
    Task<ApiResponse<UserDTO>> GetByIdAsync(Guid id);

    Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateUserDTO dto);
    Task<ApiResponse<bool>> ChangeRoleAsync(Guid id, ChangeUserRoleDTO dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
