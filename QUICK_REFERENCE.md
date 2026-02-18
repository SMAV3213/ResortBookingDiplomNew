# 🚀 Быстрый справочник для разработчика

Шпаргалка по наиболее важным концепциям, файлам и команднам Resort Booking System.

---

## 📋 Быстрые ссылки на основные файлы

### 🔐 Аутентификация
- `AuthService.cs` - Реализация JWT токенов, регистрация, вход
- `AuthController.cs` - REST endpoints для auth
- `RefreshTokenRepository.cs` - Работа с Refresh tokens в БД

### 🚪 Номера отеля
- `RoomService.cs` - CRUD операции с номерами
- `RoomRepository.cs` - Запросы к БД для номеров
- `RoomController.cs` - REST endpoints для номеров

### 📅 Бронирования
- `BookingService.cs` - Создание и управление бронированиями
- `BookingRepository.cs` - Запросы к БД для бронирований
- `BookingController.cs` - REST endpoints для бронирований

### 🤖 Автоматизация
- `RoomStatusUpdateBackgroundService.cs` - Обновляет статусы комнат каждый день
- `BookingStatusUpdateBackgroundService.cs` - Отмечает завершённые бронирования

### ⚙️ Конфигурация
- `Program.cs` - Точка входа, запуск приложения
- `DependencyInjection.cs` - Регистрация всех сервисов
- `appsettings.json` - Конфиг (БД, JWT, логирование)

---

## 🔑 Ключевые концепции

### Clean Architecture - 4 слоя

```
Domain (сущности)
    ↑
Application (интерфейсы, DTO, валидаторы)
    ↑
Infrastructure (сервисы, репозитории, БД)
    ↑
API (контроллеры, endpoints)
```

**Правило:** Зависимости всегда идут внутрь ↑

### Аутентификация - Два токена

```
AccessToken (15 мин)        RefreshToken (7 дней)
├─ Короткоживущий           ├─ Долгоживущий
├─ С каждым запросом         ├─ Сохранён в БД
├─ Содержит: ID, Role        ├─ Может быть отозван
└─ Если украдут, мало вреда  └─ Для обновления Access Token
```

### Управление жизненными циклами сервисов

```csharp
// В Program.cs
services.AddTransient<T>();    // Новый экземпляр каждый раз
services.AddScoped<T>();       // Один на HTTP request (используем чаще)
services.AddSingleton<T>();    // Один на всё приложение
```

---

## 📝 Основные DTO (форматы данных)

### Аутентификация

```csharp
// Вход
RegisterUserDTO: { login, email, phoneNumber, password }
LoginDTO: { login, password }

// Ответ
AuthResponseDTO: { accessToken, refreshToken }

// Обновление токена
RefreshTokenDTO: { refreshToken }

// Выход
LogoutDTO: { refreshToken }
```

### Номера

```csharp
// Создание
CreateRoomDTO: { number, roomTypeId }

// Обновление
UpdateRoomDTO: { number, roomTypeId, status }

// Ответ
RoomDTO: { id, number, status, roomType }
```

### Бронирования

```csharp
// Создание
CreateBookingDTO: { roomTypeId, checkIn, checkOut, guestsCount }

// Ответ
BookingDTO: { 
    id, 
    room, 
    userId, 
    checkIn, 
    checkOut, 
    guestsCount, 
    totalPrice, 
    status 
}
```

---

## 🌐 API Endpoints - Быстрая таблица

### 🔐 Auth (аутентификация)

| Метод | URL | Роль | Описание |
|-------|-----|------|---------|
| POST | `/api/auth/register` | - | Регистрация |
| POST | `/api/auth/login` | - | Вход |
| POST | `/api/auth/refresh` | - | Обновить токен |
| POST | `/api/auth/logout` | ✓ Auth | Выход |

### 👥 Users (пользователи)

| Метод | URL | Роль | Описание |
|-------|-----|------|---------|
| GET | `/api/users` | Admin | Все пользователи |
| GET | `/api/users/{id}` | Admin | Один пользователь |
| PUT | `/api/users/{id}` | Admin | Обновить |
| DELETE | `/api/users/{id}` | Admin | Удалить |

### 🏨 Room Types (типы номеров)

| Метод | URL | Роль | Описание |
|-------|-----|------|---------|
| GET | `/api/room-types` | - | Все типы |
| POST | `/api/room-types` | Admin | Создать |
| PUT | `/api/room-types/{id}` | Admin | Обновить |
| DELETE | `/api/room-types/{id}` | Admin | Удалить |

### 🚪 Rooms (номера)

| Метод | URL | Роль | Описание |
|-------|-----|------|---------|
| GET | `/api/rooms` | Admin | Все номера |
| GET | `/api/rooms/{id}` | Admin | Один номер |
| POST | `/api/rooms` | Admin | Создать |
| PUT | `/api/rooms/{id}` | Admin | Обновить |
| DELETE | `/api/rooms/{id}` | Admin | Удалить |

### 📅 Bookings (бронирования)

| Метод | URL | Роль | Описание |
|-------|-----|------|---------|
| GET | `/api/bookings` | Admin | Все бронирования |
| GET | `/api/bookings/{id}` | ✓ Auth | Одна бронь |
| GET | `/api/bookings/my` | ✓ Auth | Мои брони |
| POST | `/api/bookings` | ✓ Auth | Создать |
| POST | `/api/bookings/{id}/cancel` | ✓ Auth | Отменить |

---

## 💾 SQL команды для миграций

```bash
# Создать миграцию (автоматически найдёт изменения в моделях)
cd ResortBooking.Infrastructure
dotnet ef migrations add "MigrationName" --startup-project ..\ResortBooking.API

# Применить миграции к БД
dotnet ef database update --startup-project ..\ResortBooking.API

# Откатить последнюю миграцию
dotnet ef database update "PreviousMigrationName" --startup-project ..\ResortBooking.API

# Удалить последнюю миграцию (если ещё не применена)
dotnet ef migrations remove --startup-project ..\ResortBooking.API
```

---

## 📱 Пример: как зарегистрироваться и создать бронирование

### Шаг 1: Регистрация

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "login": "guest1",
    "email": "guest1@hotel.com",
    "phoneNumber": "+7 999 123 45 67",
    "password": "SecurePass123!"
  }'

# Ответ:
{
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "eyJ..."
  },
  "success": true
}
```

### Шаг 2: Получить типы номеров

```bash
curl https://localhost:5001/api/room-types

# Ответ:
{
  "data": [
    {
      "id": "550e8400-...",
      "name": "Стандартный номер",
      "capacity": 2,
      "pricePerNight": 100
    }
  ]
}
```

### Шаг 3: Создать бронирование

```bash
curl -X POST https://localhost:5001/api/bookings \
  -H "Authorization: Bearer eyJ..." \
  -H "Content-Type: application/json" \
  -d '{
    "roomTypeId": "550e8400-...",
    "checkIn": "2025-04-14T14:00:00Z",
    "checkOut": "2025-04-16T10:00:00Z",
    "guestsCount": 2
  }'

# Ответ:
{
  "data": "550e8400-...",  // ID созданного бронирования
  "message": "Бронирование успешно создано",
  "success": true
}
```

### Шаг 4: Получить свои бронирования

```bash
curl https://localhost:5001/api/bookings/my \
  -H "Authorization: Bearer eyJ..."

# Ответ:
{
  "data": {
    "items": [
      {
        "id": "550e8400-...",
        "room": { "number": "101", ... },
        "checkIn": "2025-04-14T14:00:00Z",
        "checkOut": "2025-04-16T10:00:00Z",
        "totalPrice": 200,
        "status": "Created"
      }
    ],
    "total": 1
  }
}
```

---

## 🐛 Частые проблемы и решения

### Ошибка: "Database connection failed"
```
Проверьте:
1. appsettings.json - правильная строка подключения
2. SQL Server запущен
3. Запустили миграции: dotnet ef database update
```

### Ошибка: "401 Unauthorized"
```
Проверьте:
1. Передали ли Authorization заголовок?
2. Правильный ли формат: Authorization: Bearer <token>
3. Не истёк ли токен?
4. Правильный ли секретный ключ в JWT конфиге?
```

### Ошибка: "Validation failed"
```
Проверьте FluentValidation правила в:
- ResortBooking.Application/Validators/
Возможно, поле не прошло валидацию
(требуется, минимальная длина и т.д.)
```

### Ошибка: "Migration pending"
```
Решение:
cd ResortBooking.Infrastructure
dotnet ef database update --startup-project ..\ResortBooking.API
```

---

## 🔍 Отладка

### Логирование

В `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning",  // EF Core логи
      "ResortBooking": "Debug"                      // Наши логи
    }
  }
}
```

### Просмотр SQL запросов

```csharp
// В ApplicationDbContext
optionsBuilder.LogTo(Console.WriteLine);  // Логирует все SQL запросы
```

### Swagger для тестирования

Откройте в браузере: `https://localhost:5001/swagger`

Здесь можно:
- 📖 Просмотреть все endpoints
- 🧪 Тестировать API прямо из браузера
- 🔐 Вставить JWT токен для авторизованных запросов

---

## 📚 Дополнительное чтение

- `ARCHITECTURE.md` - Полное описание архитектуры
- `CLEAN_ARCHITECTURE_GUIDE.md` - Глубокое объяснение Clean Architecture
- Комментарии в коде (AuthService.cs, RoomService.cs и т.д.)

---

## 🎯 Чек-лист для новых разработчиков

При подключении к проекту:

- [ ] Клонировал репозиторий
- [ ] Восстановил NuGet: `dotnet restore`
- [ ] Обновил `appsettings.json` со своими настройками
- [ ] Создал БД: `dotnet ef database update`
- [ ] Запустил проект: `dotnet run`
- [ ] Открыл Swagger: `https://localhost:5001/swagger`
- [ ] Прочитал `ARCHITECTURE.md`
- [ ] Понял структуру папок
- [ ] Посмотрел комментарии в основных сервисах
- [ ] Запустил тесты (если есть)
- [ ] Готов начать разработку! 🚀

---

**Happy Coding! 💻**

Если есть вопросы - смотри документацию в коде, ARCHITECTURE.md или CLEAN_ARCHITECTURE_GUIDE.md
