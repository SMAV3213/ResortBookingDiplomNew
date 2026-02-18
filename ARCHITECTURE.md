# 🏨 Resort Booking System - Полное руководство

Полнофункциональное REST API приложение для управления бронированиями в отелях на **ASP.NET Core 10** с использованием **Clean Architecture**.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet) ![C#](https://img.shields.io/badge/C%23-14.0-239120?style=flat-square&logo=csharp) ![SQL Server](https://img.shields.io/badge/SQL%20Server-2019%2B-CC2927?style=flat-square)

---

## 📚 Содержание

1. [Обзор архитектуры](#обзор-архитектуры)
2. [Структура проекта](#структура-проекта)
3. [Слои приложения](#слои-приложения)
4. [Как это работает](#как-это-работает)
5. [Установка](#установка)
6. [API документация](#api-документация)
7. [Примеры использования](#примеры-использования)

---

## 🏗️ Обзор архитектуры

### Clean Architecture в действии

Проект построен с использованием **Clean Architecture** - подхода, который делит приложение на независимые слои:

```
┌─────────────────────────────────┐
│   API Layer (Controllers)       │  REST endpoints
├─────────────────────────────────┤
│  Application Layer (DTOs,       │  Бизнес-логика
│  Validators, Interfaces)        │  и правила
├─────────────────────────────────┤
│  Infrastructure Layer           │  Реализация
│  (Services, Repositories)       │  (БД, сервисы)
├─────────────────────────────────┤
│  Domain Layer (Entities)        │  Сущности и enums
└─────────────────────────────────┘
```

**Зачем это нужно:**
- 🔄 **Независимость** - легко менять реализацию (БД, фреймворк и т.д.)
- ✅ **Тестируемость** - каждый слой тестируется отдельно
- 📈 **Масштабируемость** - легко добавлять новые функции
- 🧹 **Читаемость** - новому разработчику понять проект легче

---

## 📁 Структура проекта

```
ResortBookingDiplomNew/
│
├── 📦 ResortBooking.Domain/
│   ├── Entities/                    ← Сущности (User, Room, Booking...)
│   │   ├── User.cs                  (Пользователь отеля)
│   │   ├── Room.cs                  (Номер отеля)
│   │   ├── RoomType.cs              (Тип номера - люкс, стандарт и т.д.)
│   │   ├── Booking.cs               (Бронирование)
│   │   └── RefreshToken.cs          (Токен для обновления)
│   │
│   └── Enums/                       ← Перечисления
│       ├── UserRole.cs              (Admin, User)
│       ├── RoomStatus.cs            (Available, Occupied, Maintenance)
│       └── BookingStatus.cs         (Created, Confirmed, Cancelled, Completed)
│
├── 📦 ResortBooking.Application/
│   ├── Interfaces/                  ← Контракты (что должны делать сервисы)
│   ├── DTOs/                        ← Классы для передачи данных
│   ├── Validators/                  ← FluentValidation валидаторы
│   └── Responses/                   ← Единые форматы ответов
│
├── 📦 ResortBooking.Infrastructure/
│   ├── Services/                    ← Реализация бизнес-логики
│   ├── Repositories/                ← Работа с БД
│   ├── Persistence/                 ← Entity Framework контекст
│   └── BackgroundServices/          ← Автоматические задачи
│
└── 📦 ResortBooking.API/
    ├── Controllers/                 ← REST endpoints
    ├── Filters/                     ← JWT проверка
    ├── DependencyInjection.cs       ← Регистрация сервисов
    └── Program.cs                   ← Точка входа
```

---

## 🔀 Слои приложения подробно

### Domain Layer (Доменный слой)

**Отвечает за:** Описание основных сущностей бизнеса

```csharp
// User.cs - кто может пользоваться отелем
public class User
{
    public Guid Id { get; set; }
    public string Login { get; set; }           // Логин для входа
    public string Email { get; set; }           // Почта
    public string PasswordHash { get; set; }    // Пароль (в хешированном виде!)
    public UserRole Role { get; set; }          // Admin или User
}

// Room.cs - номера в отеле
public class Room
{
    public Guid Id { get; set; }
    public string Number { get; set; }          // Номер (101, 102...)
    public RoomStatus Status { get; set; }      // Available, Occupied, Maintenance
    public Guid RoomTypeId { get; set; }        // На какой тип похожа
}
```

### Application Layer (Слой приложения)

**Отвечает за:** Интерфейсы, DTO и валидацию

```csharp
// Интерфейсы - договор о том, что должны делать сервисы
public interface IAuthService
{
    Task<ApiResponse<AuthResponseDTO>> RegisterAsync(RegisterUserDTO dto);
    Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto);
}

// DTO - формат данных для передачи
public record LoginDTO(string Login, string Password);

// Валидаторы - проверка данных
public class LoginDTOValidator : AbstractValidator<LoginDTO>
{
    public LoginDTOValidator()
    {
        RuleFor(x => x.Login).NotEmpty();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}
```

### Infrastructure Layer (Слой реализации)

**Отвечает за:** Конкретная реализация сервисов и работа с БД

```csharp
// Сервис - реализация бизнес-логики
public class AuthService : IAuthService
{
    public async Task<ApiResponse<AuthResponseDTO>> LoginAsync(LoginDTO dto)
    {
        var user = await _userRepository.GetByLoginAsync(dto.Login);
        if (user == null)
            return ApiResponse<AuthResponseDTO>.Fail("Неверный логин");
        
        // Проверяем пароль и генерируем токены...
    }
}

// Репозиторий - работа с БД
public class UserRepository : IUserRepository
{
    public async Task<User?> GetByLoginAsync(string login)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Login == login);
    }
}
```

### API Layer (REST endpoints)

**Отвечает за:** REST endpoints и обработка HTTP запросов

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var response = await _authService.LoginAsync(dto);
        return response.Success ? Ok(response.Data) : BadRequest(response.Message);
    }
}
```

---

## 🔐 Как работает аутентификация

### Процесс входа

```
1. Пользователь отправляет логин и пароль
   POST /api/auth/login
   {"login": "user@example.com", "password": "Pass123!"}

2. AuthService проверяет:
   ✓ Пользователь существует?
   ✓ Пароль совпадает?

3. Генерируем два токена:

   🔴 Access Token (15 минут)
   - Короткоживущий
   - Отправляем с каждым запросом
   
   🟢 Refresh Token (7 дней)
   - Долгоживущий
   - Сохраняем в БД
   - Для получения нового Access Token

4. Отправляем оба токена клиенту
   {"accessToken": "...", "refreshToken": "..."}

5. Для следующих запросов:
   Authorization: Bearer <accessToken>
```

### Почему два токена?

- **Access Token** короткий (15 минут) - если украдут, мало времени
- **Refresh Token** долгий (7 дней) - в БД, можем отозвать в любой момент

---

## 📅 Как работает бронирование

```
1. Гость выбирает даты и количество гостей
   POST /api/bookings
   {
     "roomTypeId": "...",
     "checkIn": "2025-04-14T14:00:00Z",
     "checkOut": "2025-04-16T10:00:00Z",
     "guestsCount": 2
   }

2. BookingService проверяет:
   ✓ На эти даты есть свободный номер?
   ✓ Дата выезда позже даты входа?
   ✓ Количество гостей разумное?

3. Создаём бронирование в статусе Created

4. Бронирование переходит через статусы:
   
   Created → Confirmed → Completed (когда гость выехал)
   
   или
   
   Created → Cancelled (отменено)

5. Background Worker каждый день в 00:00:
   - Меняет статусы с Confirmed на Completed если прошла дата выезда
   - Обновляет статус номера (Occupied → Available)
```

---

## 🚀 Установка

### Требования

```
✓ .NET 10 SDK
✓ SQL Server
✓ Git (опционально)
```

### Пошагово

#### 1. Клонируем проект

```bash
git clone https://github.com/SMAV3213/ResortBookingDiplomNew.git
cd ResortBookingDiplomNew
```

#### 2. Восстанавливаем зависимости

```bash
dotnet restore
```

#### 3. Обновляем appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ResortBookingDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

#### 4. Создаём БД

```bash
cd ResortBooking.Infrastructure
dotnet ef database update --startup-project ..\ResortBooking.API
```

#### 5. Запускаем

```bash
cd ..\ResortBooking.API
dotnet run
```

Откройте: **https://localhost:5001/swagger**

---

## 📡 API Endpoints

### Аутентификация

```
POST   /api/auth/register          Регистрация
POST   /api/auth/login             Вход
POST   /api/auth/refresh           Обновить токен
POST   /api/auth/logout            Выход
```

### Номера (Admin only)

```
GET    /api/rooms                  Все номера
GET    /api/rooms/{id}             Получить номер
POST   /api/rooms                  Создать номер
PUT    /api/rooms/{id}             Обновить номер
DELETE /api/rooms/{id}             Удалить номер
```

### Бронирования

```
GET    /api/bookings               Все бронирования (Admin)
GET    /api/bookings/{id}          Получить бронирование
GET    /api/bookings/my            Мои бронирования (User)
POST   /api/bookings               Создать бронирование
POST   /api/bookings/{id}/cancel   Отменить бронирование
```

---

## 📖 Примеры использования

### Вход

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "login": "user@example.com",
    "password": "Password123!"
  }'

# Ответ:
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "base64encoded..."
  },
  "success": true
}
```

### Создать бронирование

```bash
curl -X POST https://localhost:5001/api/bookings \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "roomTypeId": "...",
    "checkIn": "2025-04-14T14:00:00Z",
    "checkOut": "2025-04-16T10:00:00Z",
    "guestsCount": 2
  }'
```

---

## 📝 Благодарности

Проект разработан как дипломная работа.

**Автор:** SMAV3213

**GitHub:** [@SMAV3213](https://github.com/SMAV3213)

---

**Статус:** ✅ Production Ready  
**Версия:** 1.0.0  
**Дата обновления:** Февраль 2025
