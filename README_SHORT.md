# 🏨 Resort Booking System

Полнофункциональное REST API для управления бронированиями в отелях, разработано на **ASP.NET Core 10** с использованием **Clean Architecture**.

## ⚡ Быстрый старт

### Требования
- .NET 10 SDK
- SQL Server (LocalDB или Express)
- Visual Studio 2022+ или VS Code

### Установка (5 минут)

```bash
# 1. Клонируем проект
git clone https://github.com/SMAV3213/ResortBookingDiplomNew.git
cd ResortBookingDiplomNew

# 2. Восстанавливаем зависимости
dotnet restore

# 3. Обновляем appsettings.json
# Отредактируйте строку подключения к БД в ResortBooking.API/appsettings.json

# 4. Создаём БД
cd ResortBooking.Infrastructure
dotnet ef database update --startup-project ..\ResortBooking.API
cd ..

# 5. Запускаем приложение
cd ResortBooking.API
dotnet run
```

Откройте в браузере: **https://localhost:5001/swagger**

## 📚 Документация

- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Полное описание архитектуры
- **[CLEAN_ARCHITECTURE_GUIDE.md](./CLEAN_ARCHITECTURE_GUIDE.md)** - Глубокое объяснение Clean Architecture
- **[DATABASE_SCHEMA.md](./DATABASE_SCHEMA.md)** - Схема БД со всеми таблицами
- **[QUICK_REFERENCE.md](./QUICK_REFERENCE.md)** - Шпаргалка для разработчиков

## 🚀 Основной функционал

### 🔐 Аутентификация
```csharp
POST /api/auth/register      // Регистрация
POST /api/auth/login         // Вход
POST /api/auth/refresh       // Обновить токен (15 мин)
POST /api/auth/logout        // Выход
```

### 🚪 Номера отеля (Admin)
```csharp
GET    /api/rooms            // Список номеров
GET    /api/rooms/{id}       // Один номер
POST   /api/rooms            // Создать
PUT    /api/rooms/{id}       // Обновить
DELETE /api/rooms/{id}       // Удалить
```

### 📅 Бронирования
```csharp
GET    /api/bookings/my      // Мои бронирования
POST   /api/bookings         // Создать бронирование
GET    /api/bookings/{id}    // Получить бронирование
POST   /api/bookings/{id}/cancel  // Отменить
```

## 🏗️ Архитектура

Четырёхслойная Clean Architecture:

```
┌─────────────────────────┐
│     API Layer           │  REST Controllers
├─────────────────────────┤
│  Application Layer      │  Interfaces, DTOs, Validators
├─────────────────────────┤
│ Infrastructure Layer    │  Services, Repositories, DB
├─────────────────────────┤
│    Domain Layer         │  Entities, Enums
└─────────────────────────┘
```

## 🔑 Ключевые особенности

✅ **JWT Аутентификация** - Access Token (15 мин) + Refresh Token (7 дней)  
✅ **Управление сессиями** - Максимум 5 активных сессий на пользователя  
✅ **Role-Based Access Control** - Admin и User роли  
✅ **Валидация данных** - FluentValidation  
✅ **Автоматические процессы** - Background Services для обновления статусов  
✅ **SQL Server** - Entity Framework Core Code-First  
✅ **Пагинация и фильтрация** - Поиск, сортировка, лимит страницы  
✅ **Swagger** - Интерактивная API документация  

## 📋 Пример использования

### 1. Регистрация

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "login": "guest1",
    "email": "guest@hotel.com",
    "phoneNumber": "+7 999 123 45 67",
    "password": "SecurePass123!"
  }'
```

### 2. Вход

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "login": "guest1",
    "password": "SecurePass123!"
  }'

# Ответ:
{
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "eyJ..."
  }
}
```

### 3. Создать бронирование

```bash
curl -X POST https://localhost:5001/api/bookings \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "roomTypeId": "550e8400-...",
    "checkIn": "2025-04-14T14:00:00Z",
    "checkOut": "2025-04-16T10:00:00Z",
    "guestsCount": 2
  }'
```

## 📁 Структура проекта

```
ResortBookingDiplomNew/
├── ResortBooking.Domain/          # Сущности и enum
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Room.cs
│   │   ├── Booking.cs
│   │   └── ...
│   └── Enums/
│       ├── UserRole.cs
│       └── BookingStatus.cs
│
├── ResortBooking.Application/     # Interfaces, DTOs, Validators
│   ├── Interfaces/
│   ├── DTOs/
│   ├── Validators/
│   └── Responses/
│
├── ResortBooking.Infrastructure/  # Services, Repositories, DB
│   ├── Services/
│   ├── Repositories/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   └── BackgroundServices/
│
└── ResortBooking.API/             # Controllers, endpoints
    ├── Controllers/
    ├── Filters/
    ├── Program.cs
    └── DependencyInjection.cs
```

## 💾 Миграции БД

```bash
# Создать миграцию
cd ResortBooking.Infrastructure
dotnet ef migrations add "MigrationName" --startup-project ..\ResortBooking.API

# Применить
dotnet ef database update --startup-project ..\ResortBooking.API

# Откатить
dotnet ef database update "PreviousMigrationName" --startup-project ..\ResortBooking.API
```

## 🧪 Тестирование

API можно тестировать через Swagger: `https://localhost:5001/swagger`

## 🔒 Безопасность

- 🔐 **Пароли хешируются** PBKDF2
- 🎫 **JWT токены подписаны** HMACSHA256
- 🔄 **Refresh Token может быть отозван**
- 📊 **Управление сессиями** - макс 5 одновременно
- ✅ **FluentValidation** - проверка всех входных данных
- 🛡️ **CORS политика** - контроль доступа между доменами

## 📖 Основные концепции

### JWT Аутентификация

```
Access Token (15 минут)
├─ Отправляется с каждым запросом
├─ Содержит: User ID, Role
└─ Если украдён - мало вреда

Refresh Token (7 дней)
├─ Сохранён в БД
├─ Используется для обновления Access Token
└─ Может быть отозван
```

### Управление бронированиями

```
Created (новое)
    ↓
Confirmed (оплачено)
    ↓
Completed (гость выехал)
    
или
    
Cancelled (отменено)
```

### Background Services

```
RoomStatusUpdateBackgroundService
└─ Каждый день в 00:00 UTC
   └─ Обновляет статусы комнат
   
BookingStatusUpdateBackgroundService
└─ Каждый день в 00:00 UTC
   └─ Отмечает завершённые бронирования
```

## 🐛 Решение проблем

### БД не подключается
```
Проверьте:
1. appsettings.json - правильная строка подключения
2. SQL Server запущен
3. Пустите миграции: dotnet ef database update
```

### 401 Unauthorized
```
Проверьте:
1. Передали ли Authorization заголовок?
2. Формат: Authorization: Bearer <token>
3. Не истёк ли токен?
```

### Ошибка валидации (400 Bad Request)
```
Проверьте FluentValidation правила в Application/Validators/
Может быть поле не прошло валидацию
```

## 🛠️ Стек технологий

- **C# 14** - Современный язык программирования
- **ASP.NET Core 10** - Web фреймворк
- **Entity Framework Core** - ORM для работы с БД
- **SQL Server** - Реляционная база данных
- **JWT** - Аутентификация
- **FluentValidation** - Валидация данных
- **Swagger/OpenAPI** - API документация
- **Clean Architecture** - Архитектура проекта

## 📚 Дополнительно

- [Microsoft Docs - ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [FluentValidation](https://fluentvalidation.net/)
- [JWT.io - Декодер токенов](https://jwt.io)

## 👨‍💻 Автор

**SMAV3213** - Студент МГТУ им. Баумана

- GitHub: [@SMAV3213](https://github.com/SMAV3213)
- Repository: [ResortBookingDiplomNew](https://github.com/SMAV3213/ResortBookingDiplomNew)

## 📝 Лицензия

MIT License - смотри LICENSE файл

---

**Версия:** 1.0.0  
**Статус:** Production Ready ✅  
**Последнее обновление:** Февраль 2025

Для полной документации смотри [ARCHITECTURE.md](./ARCHITECTURE.md)
