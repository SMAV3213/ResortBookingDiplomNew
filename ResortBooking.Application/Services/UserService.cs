using ResortBooking.Application.Auth;
using ResortBooking.Application.Interfaces;
using ResortBooking.Domain.Entities;
using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ResortBooking.Application.Services;

public class UserService
{
    private readonly IUserRepository _repo;
    private readonly IJwtTokenService _jwt;

    public UserService(IUserRepository repo, IJwtTokenService jwt)
    {
        _repo = repo;
        _jwt = jwt;
    }

    // Регистрация
    public async Task<string> RegisterAsync(string login, string password, string phoneNumber)
    {
        var existingUser = await _repo.GetByLoginAsync(login);
        if (existingUser != null)
            throw new Exception("Пользователь с таким логином уже существует");

        var user = new User
        {
            Login = login,
            PasswordHash = HashPassword(password),
            PhoneNumber = phoneNumber,
            Role = UserRole.User
        };

        await _repo.AddAsync(user);

        return "Регистрация прошла успешно";
    }

    // Логин
    public async Task<(string accessToken, string refreshToken)> LoginAsync(string login, string password)
    {
        var user = await _repo.GetByLoginAsync(login);
        if (user == null || !VerifyPassword(password, user.PasswordHash))
            throw new Exception("Неверный логин или пароль");

        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();

        return (accessToken, refreshToken);
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
