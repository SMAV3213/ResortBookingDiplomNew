using ResortBooking.Application.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.AuthDTOs;

namespace ResortBooking.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDTO>> RegisterAsync(RegisterUserDTO dto);
    Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto);
    Task<ApiResponse<AuthResponseDTO>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<string>> LogoutAsync(string refreshToken);
}
