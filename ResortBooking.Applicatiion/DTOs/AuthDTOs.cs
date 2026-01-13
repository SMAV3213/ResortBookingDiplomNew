using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.DTOs;

public class AuthDTOs
{
    public record RegisterUserDTO(
        string Login,
        string PhoneNumber,
        string Email,
        string Password
    );
    public record LoginDTO(
        string Login,
        string Password
    );
    public record AuthResponseDTO(
        string AccessToken,
        string RefreshToken
    );
    public record RefreshTokenDTO(
        string RefreshToken
    );
    public record LogoutDTO(
        string RefreshToken
    );
}
