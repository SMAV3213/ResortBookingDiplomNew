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

/// <summary>
/// Сервис аутентификации - отвечает за регистрацию, вход и управление токенами
/// 
/// Основной поток:
/// 1. Регистрация новых пользователей (хеширование пароля)
/// 2. Вход в систему (проверка пароля, выдача токенов)
/// 3. Обновление Access Token через Refresh Token
/// 4. Управление сессиями (макс 5 одновременно)
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// PasswordHasher используется для:
    /// - Хеширования пароля при регистрации
    /// - Проверки пароля при входе
    /// 
    /// Никогда не сохраняем пароль в открытом виде!
    /// Используем PBKDF2 алгоритм с солью
    /// </summary>
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

    /// <summary>
    /// Максимальное количество активных сессий для одного пользователя.
    /// Если превышено - удаляем самую старую.
    /// Это защита от утечки токенов - если пароль украли,
    /// старые сессии будут закрыты
    /// </summary>
    private const int MaxSessions = 5;
    /// <summary>
    /// Регистрация нового пользователя
    /// 
    /// Процесс:
    /// 1. Проверяем что логин, почта и телефон уникальны
    /// 2. Создаём новый объект User
    /// 3. Хешируем пароль (НИКОГДА не сохраняем в открытом виде!)
    /// 4. Сохраняем в БД и генерируем токены
    /// 
    /// Новый пользователь всегда получает роль User (не Admin)
    /// </summary>
    public async Task<ApiResponse<AuthResponseDTO>> RegisterAsync(RegisterUserDTO dto)
    {
        // Проверяем уникальность логина
        if (await _userRepository.GetByLoginAsync(dto.Login) != null)
            return ApiResponse<AuthResponseDTO>.Fail("Пользователь с таким логином уже существует");

        // Проверяем уникальность почты
        if (await _userRepository.GetByEmailAsync(dto.Email) != null)
            return ApiResponse<AuthResponseDTO>.Fail("Пользователь с такой почтой уже существует");

        // Проверяем уникальность телефона
        if (await _userRepository.GetByPhoneAsync(dto.PhoneNumber) != null)
            return ApiResponse<AuthResponseDTO>.Fail("Пользователь с таким номером телефона уже существует");

        // Создаём объект пользователя с новым ID
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Role = UserRole.User,  // Новые пользователи - обычные User, не Admin
            CreatedAt = DateTime.UtcNow
        };

        // ВАЖНО: Хешируем пароль перед сохранением
        // PasswordHasher сам добавляет соль и может быть использован многократно
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        // Сохраняем в БД
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Генерируем и возвращаем Access + Refresh токены
        return await GenerateTokensAsync(user, "Регистрация прошла успешно");
    }

    /// <summary>
    /// Вход пользователя в систему
    /// 
    /// Процесс:
    /// 1. Находим пользователя по логину в БД
    /// 2. Проверяем пароль (используем PasswordHasher для безопасного сравнения)
    /// 3. Если всё ок - генерируем токены и возвращаем
    /// 
    /// Ошибка "Неверный логин или пароль" - намеренно неспецифичная
    /// Это защита от атак подбора (не даём информацию кто точно зареган)
    /// </summary>
    public async Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto)
    {
        // Ищем пользователя по логину
        var user = await _userRepository.GetByLoginAsync(dto.Login);
        if (user == null)
            // Не даём информацию что логин не существует (защита от подбора)
            return ApiResponse<AuthResponseDTO>.Fail("Неверный логин или пароль");

        // Проверяем что пароль совпадает
        // VerifyHashedPassword сравнивает введённый пароль с хешем
        var result = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, dto.Password);

        if (result == PasswordVerificationResult.Failed)
            return ApiResponse<AuthResponseDTO>.Fail("Неверный логин или пароль");

        // Пароль верный - генерируем и отправляем токены
        return await GenerateTokensAsync(user, "Вход выполнен успешно");
    }

    /// <summary>
    /// Обновление Access Token с помощью Refresh Token
    /// 
    /// Процесс:
    /// 1. Находим Refresh Token в БД
    /// 2. Проверяем что он не отозван (revoked) и не истёк
    /// 3. Отмечаем текущий токен как revoked (больше не используем)
    /// 4. Генерируем новую пару токенов
    /// 
    /// Зачем отмечать токен как revoked?
    /// - Защита: если токен украли, старый токен больше не поможет
    /// - Даём только одну пару новых токенов за раз
    /// - Если клиент попытается ещё раз обновиться этим же токеном - ошибка
    /// </summary>
    public async Task<ApiResponse<AuthResponseDTO>> RefreshTokenAsync(string refreshToken)
    {
        // Ищем Refresh Token в БД
        var token = await _refreshTokenRepository.GetAsync(refreshToken);

        // Проверяем что токен существует, не отозван и не истёк
        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<AuthResponseDTO>.Fail("Refresh токен недействителен");

        // Отмечаем текущий токен как revoked - он больше не пригодится
        // Клиент будет использовать новый токен из ответа
        token.IsRevoked = true;
        await _refreshTokenRepository.SaveChangesAsync();

        // Генерируем новую пару токенов для клиента
        return await GenerateTokensAsync(token.User, "Токен успешно обновлён");
    }

    /// <summary>
    /// Генерирует Access Token и Refresh Token для пользователя
    /// 
    /// Ограничение сессий:
    /// - Если у пользователя > 5 активных сессий (токенов)
    /// - Удаляем самые старые
    /// - Это защита от утечки: если пароль скомпрометирован,
    ///   старые сессии будут закрыты
    /// 
    /// Два токена с разным временем жизни:
    /// 1. Access Token (15 мин) - короткоживущий, для каждого запроса
    /// 2. Refresh Token (7 дней) - долгоживущий, для обновления Access Token
    /// </summary>
    private async Task<ApiResponse<AuthResponseDTO>> GenerateTokensAsync(User user, string message)
    {
        // Подсчитываем сколько активных сессий у пользователя
        var activeSessions = await _refreshTokenRepository.CountActiveByUserIdAsync(user.Id);

        // Если сессий больше чем разрешено - удаляем самые старые
        // MaxSessions = 5, значит при 6й сессии удалим самую старую
        if (activeSessions >= MaxSessions)
        {
            await _refreshTokenRepository.DeleteOldestByUserIdAsync(
                user.Id,
                activeSessions - MaxSessions + 1);  // Удаляем лишние
        }

        // Генерируем новый Access Token (короткоживущий, действует 15 минут)
        var accessToken = GenerateAccessToken(user);

        // Генерируем криптографически стойкий Refresh Token
        var randomBytes = new byte[64];  // 64 байта = 512 бит энтропии
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);  // Заполняем случайными байтами

        // Создаём объект Refresh Token для сохранения в БД
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(randomBytes),  // Кодируем в base64
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            // Refresh Token действует 7 дней (берём из конфига)
            ExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenLifetimeDays")),
            IsRevoked = false  // Новый токен - не отозван
        };

        // Сохраняем Refresh Token в БД для последующей проверки
        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        // Возвращаем оба токена клиенту
        return ApiResponse<AuthResponseDTO>.Ok(
            new AuthResponseDTO(accessToken, refreshToken.Token),
            message
        );
    }

    /// <summary>
    /// Генерирует Access Token (JWT токен)
    /// 
    /// Access Token содержит:
    /// - Subject (sub): ID пользователя
    /// - Role: Роль (Admin или User) - используется для авторизации
    /// - Expiration: Когда истекает (15 минут)
    /// - Подпись: Подтверждает что токен не был изменён
    /// 
    /// Токен подписывается секретным ключом - если кто-то изменит содержимое,
    /// подпись станет неверной и токен будет отклонён
    /// </summary>
    private string GenerateAccessToken(User user)
    {
        // Claims - информация о пользователе внутри токена
        var claims = new[]
        {
            // Subject (sub) - стандартное поле для ID
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            // Role - используем для проверки прав доступа в контроллерах
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        // Берём секретный ключ из конфига и создаём объект для подписи
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        // Создаём JWT токен со всеми параметрами
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],              // Кто выдал (ResortBooking.API)
            audience: _configuration["Jwt:Audience"],          // Для кого (ResortBooking.Client)
            claims: claims,                                     // Содержимое
            expires: DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:AccessTokenLifetimeMinutes")),  // Когда истекает (15 мин)
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)  // Подпись
        );

        // Кодируем токен в строку (base64.base64.base64 формат)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Выход пользователя из системы
    /// 
    /// Процесс:
    /// 1. Находим Refresh Token в БД по значению
    /// 2. Отмечаем его как revoked (больше не действует)
    /// 3. Клиент больше не сможет обновить Access Token
    /// 
    /// Это мягкий выход - Access Token будет действовать до истечения (15 мин),
    /// но Refresh Token сразу становится недействительным
    /// </summary>
    public async Task<ApiResponse<string>> LogoutAsync(string refreshToken)
    {
        // Ищем Refresh Token в БД
        var token = await _refreshTokenRepository.GetAsync(refreshToken);

        // Проверяем что токен существует и ещё не отозван
        if (token == null || token.IsRevoked)
            return ApiResponse<string>.Fail("Сессия уже завершена");

        // Отмечаем токен как отозванный - больше не сможет обновиться
        token.IsRevoked = true;
        await _refreshTokenRepository.SaveChangesAsync();

        return ApiResponse<string>.Ok(
            "Вы успешно вышли из системы",
            "Выход выполнен успешно");
    }
}

