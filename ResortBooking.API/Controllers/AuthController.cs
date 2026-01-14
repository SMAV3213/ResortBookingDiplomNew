using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortBooking.Application.Interfaces.Services;
using static ResortBooking.Application.DTOs.AuthDTOs;

namespace ResortBooking.API.Controllers;

/// <summary>
/// Контроллер для управления аутентификацией пользователей (регистрация, вход, обновление токена, выход).
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AuthController"/>.
    /// </summary>
    /// <param name="authService">Сервис аутентификации.</param>
    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
    {
        var response = await _authService.RegisterAsync(dto);
        return response.Success
            ? Ok(response.Data != null ? response.Data : response.Message)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// Логин пользователя
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var response = await _authService.LoginAsync(dto);
        return response.Success
            ? Ok(response.Data != null ? response.Data : response.Message)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// Обновление Access Token через Refresh Token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDTO dto)
    {
        var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return response.Success
            ? Ok(response.Data != null ? response.Data : response.Message)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// Выход пользователя из системы (инвалидация Refresh Token)
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutDTO dto)
    {
        var response = await _authService.LogoutAsync(dto.RefreshToken);
        return response.Success
            ? Ok(response.Message)
            : BadRequest(response.Message);
    }
}
