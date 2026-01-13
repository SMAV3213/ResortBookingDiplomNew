using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static ResortBooking.Application.DTOs.AuthDTOs;

namespace ResortBooking.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(
       IUserRepository userRepository,
       IRefreshTokenRepository refreshTokenRepository,
       IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    private const int MaxSessions = 5;
    public async Task<ApiResponse<AuthResponseDTO>> RegisterAsync(RegisterUserDTO dto)
    {
        if (await _userRepository.GetByLoginAsync(dto.Login) != null)
            return ApiResponse<AuthResponseDTO>.Fail("Пользователь с таким логином уже существует");

        if (await _userRepository.GetByEmailAsync(dto.Email) != null)
            return ApiResponse<AuthResponseDTO>.Fail("Пользователь с такой почтой уже существует");

        if (await _userRepository.GetByPhoneAsync(dto.PhoneNumber) != null)
            return ApiResponse<AuthResponseDTO>.Fail("Пользователь с таким номером телефона уже существует");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return await GenerateTokensAsync(user, "Регистрация прошла успешно");
    }

    public async Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto)
    {
        var user = await _userRepository.GetByLoginAsync(dto.Login);
        if (user == null)
            return ApiResponse<AuthResponseDTO>.Fail("Неверный логин или пароль");

        var result = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, dto.Password);

        if (result == PasswordVerificationResult.Failed)
            return ApiResponse<AuthResponseDTO>.Fail("Неверный логин или пароль");

        return await GenerateTokensAsync(user, "Вход выполнен успешно");
    }

    public async Task<ApiResponse<AuthResponseDTO>> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetAsync(refreshToken);
        if (token == null ||
        token.IsRevoked ||
        token.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<AuthResponseDTO>.Fail("Refresh токен недействителен");

        token.IsRevoked = true;
        await _refreshTokenRepository.SaveChangesAsync();

        return await GenerateTokensAsync(token.User, "Токен успешно обновлён");
    }

    private async Task<ApiResponse<AuthResponseDTO>> GenerateTokensAsync(
    User user,
    string message)
    {
        var activeSessions =
            await _refreshTokenRepository.CountActiveByUserIdAsync(user.Id);

        if (activeSessions >= MaxSessions)
        {
            await _refreshTokenRepository.DeleteOldestByUserIdAsync(
                user.Id,
                activeSessions - MaxSessions + 1);
        }

        var accessToken = GenerateAccessToken(user);

        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(randomBytes),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenLifetimeDays")),
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        return ApiResponse<AuthResponseDTO>.Ok(
            new AuthResponseDTO(accessToken, refreshToken.Token),
            message
        );
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:AccessTokenLifetimeMinutes")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<ApiResponse<string>> LogoutAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetAsync(refreshToken);

        if (token == null || token.IsRevoked)
            return ApiResponse<string>.Fail("Сессия уже завершена");

        token.IsRevoked = true;
        await _refreshTokenRepository.SaveChangesAsync();

        return ApiResponse<string>.Ok(
            "Вы успешно вышли из системы",
            "Выход выполнен успешно");
    }
}

