# 📚 Clean Architecture - Подробное объяснение

Этот документ объясняет архитектуру проекта Resort Booking на примерах из реального кода.

---

## 🎯 Что такое Clean Architecture?

Clean Architecture - это подход к организации кода, который делит приложение на независимые слои:

```
┌─────────────────────────────────────────────────────┐
│                                                       │
│              ENTERPRISE BUSINESS RULES                │
│                                                       │
├─────────────────────────────────────────────────────┤
│                                                       │
│           APPLICATION BUSINESS RULES                  │
│         (Use Cases, Interfaces, Validators)          │
│                                                       │
├─────────────────────────────────────────────────────┤
│                                                       │
│        INTERFACE ADAPTERS                             │
│     (Controllers, Presenters, Repositories)          │
│                                                       │
├─────────────────────────────────────────────────────┤
│                                                       │
│          FRAMEWORKS & DRIVERS                        │
│   (Web, DB, UI, External Services)                   │
│                                                       │
└─────────────────────────────────────────────────────┘
```

**Правило важности:**
- Внутренние слои НЕ знают о внешних
- Внешние слои знают о внутренних
- Зависимости всегда направлены внутрь

---

## 🏗️ Наша структура проекта

### Domain Layer (Доменный слой) - самый внутренний

**Отвечает за:** Основные сущности и бизнес-правила

```csharp
// ResortBooking.Domain/Entities/User.cs
public class User
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }  // ХЕШИРОВАН!
    public string PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ResortBooking.Domain/Enums/UserRole.cs
public enum UserRole
{
    User = 0,
    Admin = 1
}
```

**Характеристики Domain Layer:**
- ✅ НЕ зависит ни от чего (от другого слоя)
- ✅ Содержит только сущности и enum
- ✅ Можно использовать в любом проекте (мобильное приложение, консоль и т.д.)
- ✅ Не знает про HTTP, БД, JWT

---

### Application Layer (Слой приложения)

**Отвечает за:** Интерфейсы (контракты), DTO (форматы данных), валидацию

```csharp
// Интерфейсы - определяют ЧТО должны делать сервисы
// ResortBooking.Application/Interfaces/Services/IAuthService.cs
public interface IAuthService
{
    Task<ApiResponse<AuthResponseDTO>> RegisterAsync(RegisterUserDTO dto);
    Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto);
    Task<ApiResponse<AuthResponseDTO>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
}

// DTO - как передаём данные от клиента к серверу
// ResortBooking.Application/DTOs/AuthDTOs.cs
public record LoginDTO(
    string Login,
    string Password
);

public record AuthResponseDTO(
    string AccessToken,
    string RefreshToken
);

// Валидаторы - проверяем входные данные ДО обработки
// ResortBooking.Application/Validators/LoginDTOValidator.cs
public class LoginDTOValidator : AbstractValidator<LoginDTO>
{
    public LoginDTOValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Логин обязателен")
            .MinimumLength(3).WithMessage("Логин минимум 3 символа");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(8).WithMessage("Пароль минимум 8 символов");
    }
}

// Единый формат ответа
// ResortBooking.Application/Responses/ApiResponse.cs
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string Message { get; set; } = "";
    public bool Success { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "")
        => new() { Data = data, Message = message, Success = true, StatusCode = 200 };

    public static ApiResponse<T> Fail(string message)
        => new() { Message = message, Success = false, StatusCode = 400 };
}
```

**Зависимости Application Layer:**
- ✓ Зависит от Domain Layer
- ✗ НЕ зависит от Infrastructure
- ✗ НЕ зависит от API

**Преимущества:**
- Application слой можно тестировать без БД
- Легко менять реализацию Infrastructure (например, с SQL на NoSQL)

---

### Infrastructure Layer (Слой реализации)

**Отвечает за:** Конкретная реализация сервисов, работа с БД, внешние сервисы

```csharp
// РЕАЛИЗАЦИЯ интерфейса из Application Layer
// ResortBooking.Infrastructure/Services/AuthService.cs
public class AuthService : IAuthService  // ← Реализуем контракт
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;  // ← Dependency Injection
    }

    // Регистрация нового пользователя
    public async Task<ApiResponse<AuthResponseDTO>> RegisterAsync(RegisterUserDTO dto)
    {
        // 1. Проверяем что логин уникален
        if (await _userRepository.GetByLoginAsync(dto.Login) != null)
            return ApiResponse<AuthResponseDTO>.Fail("Логин уже используется");

        // 2. Создаём пользователя
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            Email = dto.Email,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        // 3. ОЧЕНЬ ВАЖНО: хешируем пароль перед сохранением
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
        // Никогда не сохраняем пароль в открытом виде!

        // 4. Сохраняем в БД
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // 5. Генерируем токены и возвращаем
        return await GenerateTokensAsync(user, "Регистрация успешна");
    }

    // Проверка пароля при входе
    public async Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto)
    {
        var user = await _userRepository.GetByLoginAsync(dto.Login);
        if (user == null)
            return ApiResponse<AuthResponseDTO>.Fail("Неверный логин или пароль");

        // Используем PasswordHasher для безопасного сравнения
        var result = _passwordHasher.VerifyHashedPassword(
            user, 
            user.PasswordHash,  // ← Хеш из БД
            dto.Password        // ← Введённый пользователем пароль
        );

        if (result == PasswordVerificationResult.Failed)
            return ApiResponse<AuthResponseDTO>.Fail("Неверный логин или пароль");

        return await GenerateTokensAsync(user, "Вход успешен");
    }
}

// РЕАЛИЗАЦИЯ работы с БД
// ResortBooking.Infrastructure/Repositories/UserRepository.cs
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;  // Entity Framework DbContext
    }

    // Получить пользователя по логину
    public async Task<User?> GetByLoginAsync(string login)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Login == login);
    }

    // Добавить пользователя
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    // Сохранить все изменения в БД (транзакция)
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
```

**Зависимости Infrastructure Layer:**
- ✓ Зависит от Domain Layer
- ✓ Зависит от Application Layer
- ✓ Зависит от внешних библиотек (EF Core, JWT и т.д.)

---

### API Layer (REST API)

**Отвечает за:** HTTP endpoints, получение запросов от клиента

```csharp
// ResortBooking.API/Controllers/AuthController.cs
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;  // ← Используем сервис

    public AuthController(IAuthService authService)
    {
        _authService = authService;  // ← Dependency Injection
    }

    /// <summary>
    /// POST /api/auth/register
    /// Регистрирует нового пользователя
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
    {
        // FluentValidation проверит dto ДО того как попадёт сюда
        // Если ошибки - вернёт 400 Bad Request автоматически

        var response = await _authService.RegisterAsync(dto);

        // Возвращаем ответ клиенту
        return response.Success
            ? Ok(response.Data)              // 200 OK с токенами
            : BadRequest(response.Message);  // 400 Bad Request с ошибкой
    }

    /// <summary>
    /// POST /api/auth/login
    /// Вход в систему
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var response = await _authService.LoginAsync(dto);
        return response.Success
            ? Ok(response.Data)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// POST /api/auth/refresh
    /// Обновление Access Token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDTO dto)
    {
        var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return response.Success
            ? Ok(response.Data)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// POST /api/auth/logout
    /// Выход из системы (требует авторизации)
    /// </summary>
    [Authorize]  // ← Требуем валидный JWT токен
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutDTO dto)
    {
        var response = await _authService.LogoutAsync(dto.RefreshToken);
        return response.Success
            ? Ok(response.Message)
            : BadRequest(response.Message);
    }
}
```

---

## 🔄 Процесс запроса от клиента

```
1. Клиент отправляет HTTP запрос
   ↓
   POST /api/auth/login
   Content-Type: application/json
   {
     "login": "user@example.com",
     "password": "Password123!"
   }

2. ASP.NET роутит на AuthController.Login()
   ↓

3. FluentValidation проверяет LoginDTO
   ✓ Логин не пустой?
   ✓ Пароль минимум 8 символов?
   Если ошибки → вернуть 400 Bad Request

4. AuthService.LoginAsync() выполняет бизнес-логику
   ↓
   - Ищет пользователя в БД (через UserRepository)
   - Проверяет пароль (безопасное сравнение)
   - Если ок → генерирует JWT токены

5. Возвращаем ответ клиенту
   ↓
   200 OK
   {
     "data": {
       "accessToken": "eyJ...",
       "refreshToken": "eyJ..."
     },
     "success": true
   }

6. Клиент сохраняет токены и отправляет AccessToken с каждым запросом
   ↓
   Authorization: Bearer eyJ...
```

---

## 🔐 Безопасность - Пароли

### ❌ НЕПРАВИЛЬНО: сохранять пароль в открытом виде

```csharp
// ОПАСНО! Если БД украдут, все пароли скомпрометированы
user.Password = "MyPassword123";
await _context.SaveChangesAsync();
```

### ✅ ПРАВИЛЬНО: хешировать пароль

```csharp
// БЕЗОПАСНО! Хеш невозможно обратить в пароль
var hasher = new PasswordHasher<User>();
user.PasswordHash = hasher.HashPassword(user, "MyPassword123");
await _context.SaveChangesAsync();

// При входе:
var result = hasher.VerifyHashedPassword(user, user.PasswordHash, "MyPassword123");
if (result == PasswordVerificationResult.Success)
{
    // Пароль верный!
}
```

**Что происходит:**
1. Берём пароль "MyPassword123"
2. Добавляем к нему случайную "соль" (random bytes)
3. Хешируем (PBKDF2): 100000 итераций хеширования
4. Результат: "$2a$11$N9qo8uLO..." - невозможно восстановить пароль

---

## 🎫 Безопасность - JWT токены

### Что такое JWT?

JWT (JSON Web Token) состоит из трёх частей:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9 . eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ . SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
┌─────────────────────────────────────┬────────────────────────────────────┬─────────────────────────────────┐
│ Header (заголовок)                  │ Payload (содержимое)               │ Signature (подпись)             │
│ {"alg":"HS256","typ":"JWT"}         │ {"sub":"...", "role":"Admin"...}   │ HMACSHA256(header.payload,key)  │
└─────────────────────────────────────┴────────────────────────────────────┴─────────────────────────────────┘
```

**Как это защищает:**

1. **Если кто-то попытается изменить содержимое:**
   - Изменит payload: `{"role":"Admin"}` → `{"role":"SuperAdmin"}`
   - Подпись не совпадёт → токен отклонён!

2. **Если кто-то попытается подделать подпись:**
   - Нужно знать секретный ключ
   - Ключ только на сервере

```csharp
// Генерируем токен
var token = new JwtSecurityToken(
    issuer: "ResortBooking.API",              // Кто выдал
    audience: "ResortBooking.Client",         // Для кого
    claims: new[]                             // Содержимое
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role.ToString())  // Role для авторизации
    },
    expires: DateTime.UtcNow.AddMinutes(15),  // Действует 15 минут
    signingCredentials: new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret-key")),
        SecurityAlgorithms.HmacSha256
    )
);

// Преобразуем в строку
var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
// Результат: eyJhbGciOiJIUzI1NiIs...

// Проверка токена на сервере
var principal = new JwtSecurityTokenHandler().ValidateToken(
    tokenString,
    new TokenValidationParameters
    {
        ValidIssuer = "ResortBooking.API",
        ValidAudience = "ResortBooking.Client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret-key"))
    },
    out SecurityToken validatedToken
);

if (principal != null)
{
    var role = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
    // role = "Admin" или "User"
}
```

---

## 📅 Dependency Injection (внедрение зависимостей)

### Проблема без DI:

```csharp
// ❌ ПЛОХО: жёсткая связь между классами
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController()
    {
        // Контроллер сам создаёт сервис - тесно связано!
        _authService = new AuthService(
            new UserRepository(new ApplicationDbContext()),
            new RefreshTokenRepository(new ApplicationDbContext())
        );
    }
}
```

**Проблемы:**
- Трудно тестировать (нельзя подменить на mock)
- Если изменится конструктор AuthService - нужно менять везде
- Сложно масштабировать

### Решение с DI:

```csharp
// ✅ ХОРОШО: зависимости внедряются извне
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // IoC контейнер автоматически внедрит зависимость
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
}

// В Program.cs регистрируем:
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IUserRepository, UserRepository>();
```

**Преимущества:**
- ✅ Слабая связь (через интерфейсы)
- ✅ Легко тестировать (подставим mock)
- ✅ Легко менять реализацию
- ✅ ASP.NET сам создаст и передаст объект

---

## 🧪 Тестирование с использованием Clean Architecture

```csharp
// Тест AuthService БЕЗ БД - используем mock
[TestClass]
public class AuthServiceTests
{
    private Mock<IUserRepository> _mockUserRepository;
    private AuthService _authService;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        
        // Создаём сервис с mock репозиторием
        _authService = new AuthService(_mockUserRepository.Object);
    }

    [TestMethod]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResponse()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), PasswordHash = "..." };
        _mockUserRepository
            .Setup(r => r.GetByLoginAsync("test"))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(
            new LoginDTO("test", "password")
        );

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data?.AccessToken);
    }
}
```

**Зачем это работает:**
- IAuthService - интерфейс, не привязан к конкретной реализации
- Mock - подменяет IUserRepository
- Тестируем AuthService изолированно от БД
- Быстро, надёжно, не требует БД

---

## 📊 Диаграмма зависимостей

```
                    API Layer
                   (External)
                        │
                        ↓
                   Controllers
                        │
                        ↓
          ┌─────────────┴────────────┐
          ↓                          ↓
     Application Layer         Infrastructure Layer
     (Use Cases)               (Implementation)
          │                          │
          ├─ Interfaces             ├─ Services (реализация)
          ├─ DTOs                   ├─ Repositories (БД)
          ├─ Validators             ├─ DbContext
          └─ Responses              └─ External APIs
          │                          │
          └─────────────┬────────────┘
                        ↓
                  Domain Layer
                  (Entities, Enums)
                  (Core Business)
```

**Правила потока зависимостей:**
- ✅ API может использовать Infrastructure
- ✅ Infrastructure использует Application
- ✅ Application использует Domain
- ✅ Domain не использует ничего
- ✗ Domain НЕ может использовать другие слои
- ✗ Application НЕ может использовать Infrastructure напрямую (только через интерфейсы!)

---

## 🎓 Заключение

Clean Architecture помогает:

1. **📚 Организовать код** - каждый слой отвечает за свою работу
2. **🧪 Тестировать** - слои независимы, легко использовать mock
3. **🔄 Масштабировать** - легко добавлять новые функции
4. **🛡️ Безопасность** - явные зависимости, контроль доступа
5. **👥 Командная работа** - разработчикам легче разобраться в коде
6. **🔧 Поддержка** - проще находить и исправлять баги

**Ключевая идея:** Высокоуровневые политики (бизнес-логика) не должны зависеть от низкоуровневых деталей (БД, фреймворк). Это достигается через использование интерфейсов и инверсии зависимостей.

---

**Создано для понимания архитектуры Resort Booking System**  
**Дипломная работа МГТУ им. Баумана**
