using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Auth;
using ResortBooking.Domain.Entities;
using ResortBooking.Domain.Enums;
using ResortBooking.Infrastructure.Auth;
using ResortBooking.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace ResortBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ResortBookingDbContext _db;
    private readonly IJwtTokenService _jwt;

    public UsersController(ResortBookingDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Login == request.Login))
            return BadRequest("Пользователь с таким логином уже существует");

        var user = new User
        {
            Login = request.Login,
            PasswordHash = HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            Role = UserRole.User
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok("Регистрация прошла успешно");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Неверный логин или пароль");

        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();

        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}

public record RegisterRequest(string Login, string Password, string PhoneNumber);
public record LoginRequest(string Login, string Password);
