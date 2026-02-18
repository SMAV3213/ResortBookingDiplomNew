# 🏨 Resort Booking System

Полнофункциональное REST API для управления бронированиями в отелях, разработано на **ASP.NET Core 10** с использованием **Clean Architecture**.

---

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

---

## 📚 Структура проекта

```
ResortBookingDiplomNew/
│
├── ResortBooking.API/                    # REST API слой (контроллеры, middleware)
│   ├── Controllers/                      # Endpoints приложения
│   ├── DependencyInjection.cs            # Регистрация сервисов
│   ├── Program.cs                        # Точка входа
│   └── appsettings.json                  # Конфигурация БД и JWT
│
├── ResortBooking.Application/            # Бизнес-логика (SOLID принципы)
│   ├── DTOs/                             # Data Transfer Objects
│   ├── Validators/                       # FluentValidation валидаторы
│   ├── Services/                         # Интерфейсы сервисов
│   └── Data/Options/                     # Конфигурационные опции
│
├── ResortBooking.Infrastructure/         # Реализация БД и внешних сервисов
│   ├── Data/
│   │   ├── AppDbContext.cs               # EF Core контекст
│   │   ├── Options/                      # Реализация опций
│   │   └── Migrations/                   # БД миграции
│   ├── Services/                         # Реализация сервисов
│   ├── BackgroundServices/               # Background tasks
│   ├── Repositories/                     # Репозитории для работы с БД
│   └── DependencyInjection.cs            # Регистрация сервисов
│
└── ResortBooking.Domain/                 # Доменные сущности (не меняются)
    ├── Entities/                         # User, Booking, Resort и т.д.
    └── Enums/                            # BookingStatus, RoomType и т.д.
```

---

## 🏗️ Архитектура

### Clean Architecture - разделение ответственности

```
┌─────────────────────────────────┐
│   API Layer (Controllers)       │  HTTP endpoints (GET, POST, PUT, DELETE)
├─────────────────────────────────┤
│  Application Layer              │  Бизнес-логика, валидация,
│  (DTOs, Validators)             │  правила приложения
├─────────────────────────────────┤
│  Infrastructure Layer           │  БД, сервисы, репозитории,
│  (Services, Repositories)       │  внешние API
├─────────────────────────────────┤
│  Domain Layer (Entities)        │  Сущности, enums, правила
└─────────────────────────────────┘
```

**Почему Clean Architecture?**
- 🔄 **Независимость** - фреймворк/БД легко заменить
- ✅ **Тестируемость** - каждый слой тестируется отдельно  
- 📈 **Масштабируемость** - просто добавлять новые функции
- 🧹 **Читаемость** - новый разработчик быстро разберётся

---

## 🚀 Основной функционал

### 🔐 Аутентификация (JWT)
```
POST   /api/auth/register       Регистрация нового пользователя
POST   /api/auth/login          Вход в систему (возврат JWT токена)
POST   /api/auth/refresh        Обновить токен (длина 15 минут)
```

**Как работает:**
1. Пользователь отправляет логин/пароль
2. Сервер создает JWT токен (содержит ID пользователя, роль)
3. Клиент отправляет токен в заголовке Authorization
4. Сервер проверяет подпись и роль пользователя

### 🏩 Управление отелями
```
GET    /api/resorts             Получить все отели
GET    /api/resorts/{id}        Получить отель по ID
POST   /api/resorts             Создать отель (только админ)
PUT    /api/resorts/{id}        Обновить отель (только админ)
DELETE /api/resorts/{id}        Удалить отель (только админ)
```

### 🛏️ Управление номерами
```
GET    /api/rooms               Получить все номера
GET    /api/rooms/{id}          Получить номер по ID
POST   /api/rooms               Создать номер
PUT    /api/rooms/{id}          Обновить номер
```

### 📅 Бронирования
```
GET    /api/bookings            Получить мои бронирования
POST   /api/bookings            Создать бронирование
PUT    /api/bookings/{id}       Обновить бронирование
DELETE /api/bookings/{id}       Отменить бронирование
```

**Автоматическое обновление статуса:**
- 🕐 Background Service каждый час проверяет бронирования
- 📋 При истечении даты заезда статус меняется на `Completed`
- ❌ Неподтвержденные бронирования удаляются через 24 часа

---

## 🔑 Ключевые компоненты

### 1. AppOptions - Конфигурация приложения
Хранит пути к папкам, CORS origins и другие настройки:
```csharp
// Инициализируется в Program.cs
var appOptions = new AppOptions();
appOptions.InitalizeOptions(app.Environment.ContentRootPath);
// Пути вычисляются: wwwroot/additional-files/
```

### 2. JWT токены - Безопасная аутентификация
```csharp
// Секретный ключ подписывает каждый токен
// Клиент не может подделать токен без ключа
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // ...
})
```

### 3. Background Services - Автоматические процессы
Сервис `BookingStatusUpdateBackgroundService` запускается в фоне и:
- Проверяет статусы бронирований каждый час
- Обновляет завершенные бронирования
- Удаляет истекшие неподтвержденные бронирования

### 4. FluentValidation - Валидация DTO
```csharp
// Валидаторы автоматически применяются к DTO
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
// Если DTO невалидна, возвращается 400 Bad Request с деталями
```

---

## 🗄️ База данных

### Основные таблицы

**Users** - Пользователи системы
```
Id (PK) | Email | PasswordHash | FirstName | LastName | Role
```

**Resorts** - Отели
```
Id (PK) | Name | City | Address | Description | Rating
```

**Rooms** - Номера в отелях
```
Id (PK) | ResortId (FK) | RoomNumber | Type | Capacity | PricePerNight
```

**Bookings** - Бронирования
```
Id (PK) | UserId (FK) | RoomId (FK) | CheckIn | CheckOut | Status
```

**Reviews** - Отзывы о номерах
```
Id (PK) | RoomId (FK) | UserId (FK) | Rating | Comment
```

---

## 🔧 Установка и конфигурация

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ResortBookingDb;Trusted_Connection=true"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-min-32-chars",
    "Issuer": "ResortBooking",
    "Audience": "ResortBookingClients",
    "ExpirationMinutes": 15
  },
  "AppOptions": {
    "AdditionalFilesDirectoryName": "additional-files"
  }
}
```

### Entity Framework миграции
```bash
# Создать новую миграцию
dotnet ef migrations add MigrationName --project ResortBooking.Infrastructure --startup-project ResortBooking.API

# Применить миграции
dotnet ef database update --startup-project ResortBooking.API

# Откатить последнюю миграцию
dotnet ef migrations remove --project ResortBooking.Infrastructure
```

---

## 📖 Как разработать новую фичу

### 1️⃣ Создайте Entity в Domain Layer
```csharp
// ResortBooking.Domain/Entities/NewEntity.cs
public class NewEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    // ...
}
```

### 2️⃣ Создайте DTO в Application Layer
```csharp
// ResortBooking.Application/DTOs/NewEntityDto.cs
public class CreateNewEntityDto
{
    public string Name { get; set; }
}
```

### 3️⃣ Создайте Validator
```csharp
// ResortBooking.Application/Validators/CreateNewEntityValidator.cs
public class CreateNewEntityValidator : AbstractValidator<CreateNewEntityDto>
{
    public CreateNewEntityValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

### 4️⃣ Создайте Service Interface
```csharp
// ResortBooking.Application/Services/INewEntityService.cs
public interface INewEntityService
{
    Task<NewEntityDto> CreateAsync(CreateNewEntityDto dto);
}
```

### 5️⃣ Реализуйте Service
```csharp
// ResortBooking.Infrastructure/Services/NewEntityService.cs
public class NewEntityService : INewEntityService
{
    public async Task<NewEntityDto> CreateAsync(CreateNewEntityDto dto)
    {
        // Логика создания
    }
}
```

### 6️⃣ Создайте Controller
```csharp
// ResortBooking.API/Controllers/NewEntitiesController.cs
[ApiController]
[Route("api/[controller]")]
public class NewEntitiesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNewEntityDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return Ok(result);
    }
}
```

### 7️⃣ Добавьте миграцию БД
```bash
dotnet ef migrations add AddNewEntity --project ResortBooking.Infrastructure
dotnet ef database update
```

---

## 🧪 Тестирование API

### Swagger UI (встроенная документация)
```
https://localhost:5001/swagger
```

### Postman примеры

**Регистрация:**
```
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Вход:**
```
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

Ответ содержит JWT токен - добавьте его в заголовок:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## ⚙️ Развертывание

### Docker (опционально)
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/ResortBooking.API.dll"]
```

### GitHub Actions CI/CD
Проект автоматически тестируется и собирается при каждом push.

---

## 📝 Соглашения кода

### Именование
- **Classes**: `PascalCase` (BookingService)
- **Methods**: `PascalCase` (GetBookings)
- **Variables**: `camelCase` (bookingId)
- **Constants**: `UPPER_CASE` (MAX_RETRIES)

### Entity Framework
- Каждый entity должен наследовать `BaseEntity` с `Id`
- Используйте Data Annotations или Fluent API для конфигурации
- Всегда создавайте миграцию после изменения моделей

### API Responses
```csharp
// Успешный ответ (200 OK)
return Ok(new { message = "Success", data = result });

// Ошибка валидации (400 Bad Request)
return BadRequest(new { message = "Invalid data" });

// Не найдено (404 Not Found)
return NotFound();

// Нет прав (403 Forbidden)
return Forbid();
```

---

## 🐛 Решение проблем

### БД не синхронизируется
```bash
# Откатить все миграции и пересоздать БД
dotnet ef database drop --project ResortBooking.Infrastructure --force
dotnet ef database update --startup-project ResortBooking.API
```

### Ошибка подключения к SQL Server
Проверьте `appsettings.json`:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ResortBookingDb;Trusted_Connection=true"
```

### JWT токен не работает
Убедитесь, что:
1. Токен передан в заголовке `Authorization: Bearer <token>`
2. `SecretKey` совпадает в `appsettings.json` и коде
3. Токен не истек

---

## 📞 Контакты и помощь

- **Git Issues** - Для багов и фичей
- **Pull Requests** - Приветствуются контрибьюции
- **Documentation** - Смотрите папку /docs

