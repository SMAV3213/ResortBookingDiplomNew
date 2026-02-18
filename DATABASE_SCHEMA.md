# 🗄️ Структура базы данных

Полное описание всех таблиц, связей и типов данных в Resort Booking System.

---

## 📊 Entity Relationship Diagram (ERD)

```
┌──────────────────┐
│     Users        │
├──────────────────┤
│ Id (PK, GUID)    │
│ Login (string)   │
│ Email (string)   │
│ PasswordHash (s) │
│ PhoneNumber (s)  │
│ Role (int enum)  │
│ CreatedAt (dt)   │
└────────┬─────────┘
         │ 1:N
         │
    ┌────┴─────────────┬─────────────────┐
    │                  │                 │
┌───▼──────────────┐  │  ┌──────────────▼──┐
│ RefreshTokens    │  │  │    Bookings      │
├──────────────────┤  │  ├─────────────────┤
│ Id (PK, GUID)    │  │  │ Id (PK, GUID)   │
│ Token (string)   │  │  │ RoomId (FK)     │
│ UserId (FK)      ├──┘  │ UserId (FK)     ├──┐
│ CreatedAt (dt)   │     │ CheckInDate(dt) │  │
│ ExpiresAt (dt)   │     │ CheckOutDate(dt)│  │
│ IsRevoked (bool) │     │ GuestsCount(int)│  │
└──────────────────┘     │ TotalPrice(dec) │  │
                         │ Status (int)    │  │
                         │ CreatedAt (dt)  │  │
                         └─────────────────┘  │
                                              │
    ┌─────────────────────────────────────────┘
    │
┌───▼───────────────┐
│     Rooms         │
├───────────────────┤
│ Id (PK, GUID)     │
│ Number (string)   │
│ Status (int enum) │
│ RoomTypeId (FK)   ├──┐
│ CreatedAt (dt)    │  │
└───────────────────┘  │
                       │
    ┌──────────────────┘
    │
┌───▼──────────────────┐
│    RoomTypes         │
├──────────────────────┤
│ Id (PK, GUID)        │
│ Name (string)        │
│ Description (string) │
│ Capacity (int)       │
│ PricePerNight (dec)  │
│ CreatedAt (dt)       │
└──────────────────────┘
```

---

## 📝 Таблицы

### 1. Users (Пользователи)

Хранит информацию о всех пользователях системы.

```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Login NVARCHAR(MAX) NOT NULL UNIQUE,
    Email NVARCHAR(MAX) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,      -- НИКОГДА не пароль в открытом виде!
    PhoneNumber NVARCHAR(MAX) NOT NULL UNIQUE,
    Role INT NOT NULL,                         -- 0 = User, 1 = Admin
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
);

-- Индексы для быстрого поиска
CREATE INDEX IX_Users_Login ON Users(Login);
CREATE INDEX IX_Users_Email ON Users(Email);
```

**Поля:**

| Поле | Тип | Описание |
|------|-----|---------|
| Id | GUID | Уникальный идентификатор |
| Login | string | Логин для входа (уникален) |
| Email | string | Почта (уникальна) |
| PasswordHash | string | Хеш пароля PBKDF2 |
| PhoneNumber | string | Номер телефона (уникален) |
| Role | int enum | 0 = обычный User, 1 = Admin |
| CreatedAt | datetime | Когда создан (UTC) |

**Примеры:**

```
Id: 550e8400-e29b-41d4-a716-446655440000
Login: john_doe
Email: john@example.com
PasswordHash: $2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7Ey
PhoneNumber: +7 999 123 45 67
Role: 0 (User)
CreatedAt: 2025-02-01 10:30:00
```

---

### 2. RoomTypes (Типы номеров)

Хранит информацию о категориях номеров в отеле.

```sql
CREATE TABLE RoomTypes (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX),
    Capacity INT NOT NULL,                    -- На скольких человек
    PricePerNight DECIMAL(18, 2) NOT NULL,    -- Цена за ночь
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
);
```

**Поля:**

| Поле | Тип | Описание |
|------|-----|---------|
| Id | GUID | Уникальный идентификатор |
| Name | string | Название (Люкс, Стандарт и т.д.) |
| Description | string | Описание (удобства, вид и т.д.) |
| Capacity | int | Количество мест |
| PricePerNight | decimal | Цена за ночь |
| CreatedAt | datetime | Когда создан |

**Примеры:**

```
Id: 550e8400-e29b-41d4-a716-446655440001
Name: Стандартный номер
Description: Уютный номер с двуспальной кроватью
Capacity: 2
PricePerNight: 150.00
CreatedAt: 2025-01-15 08:00:00

Id: 550e8400-e29b-41d4-a716-446655440002
Name: Люкс номер
Description: Роскошный номер с гостиной и террасой
Capacity: 4
PricePerNight: 500.00
CreatedAt: 2025-01-15 08:00:00
```

---

### 3. Rooms (Номера)

Хранит информацию о каждом номере в отеле.

```sql
CREATE TABLE Rooms (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Number NVARCHAR(MAX) NOT NULL UNIQUE,     -- "101", "102"...
    Status INT NOT NULL,                       -- 0 = Available, 1 = Occupied, 2 = Maintenance
    RoomTypeId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(Id) ON DELETE RESTRICT
);

CREATE INDEX IX_Rooms_Number ON Rooms(Number);
CREATE INDEX IX_Rooms_Status ON Rooms(Status);
CREATE INDEX IX_Rooms_RoomTypeId ON Rooms(RoomTypeId);
```

**Поля:**

| Поле | Тип | Описание |
|------|-----|---------|
| Id | GUID | Уникальный идентификатор |
| Number | string | Номер номера (уникален) |
| Status | int enum | 0 = Available, 1 = Occupied, 2 = Maintenance |
| RoomTypeId | GUID | Ссылка на тип номера |
| CreatedAt | datetime | Когда создан |

**Примеры:**

```
Id: 550e8400-e29b-41d4-a716-446655440010
Number: 101
Status: 0 (Available)
RoomTypeId: 550e8400-e29b-41d4-a716-446655440001
CreatedAt: 2025-01-20 12:00:00

Id: 550e8400-e29b-41d4-a716-446655440011
Number: 102
Status: 1 (Occupied)
RoomTypeId: 550e8400-e29b-41d4-a716-446655440001
CreatedAt: 2025-01-20 12:00:00
```

**Статусы:**
- 0 = Available (свободен)
- 1 = Occupied (занят)
- 2 = Maintenance (на техническом обслуживании)

---

### 4. Bookings (Бронирования)

Хранит информацию о всех бронированиях.

```sql
CREATE TABLE Bookings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RoomId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CheckInDate DATETIME NOT NULL,
    CheckOutDate DATETIME NOT NULL,
    GuestsCount INT NOT NULL,
    TotalPrice DECIMAL(18, 2) NOT NULL,
    Status INT NOT NULL,                      -- 0 = Created, 1 = Confirmed, 2 = Cancelled, 3 = Completed
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Bookings_UserId ON Bookings(UserId);
CREATE INDEX IX_Bookings_RoomId ON Bookings(RoomId);
CREATE INDEX IX_Bookings_Status ON Bookings(Status);
CREATE INDEX IX_Bookings_CheckInDate ON Bookings(CheckInDate);
CREATE INDEX IX_Bookings_CheckOutDate ON Bookings(CheckOutDate);
```

**Поля:**

| Поле | Тип | Описание |
|------|-----|---------|
| Id | GUID | Уникальный идентификатор |
| RoomId | GUID | Ссылка на номер |
| UserId | GUID | Ссылка на пользователя |
| CheckInDate | datetime | Дата и время заезда |
| CheckOutDate | datetime | Дата и время выезда |
| GuestsCount | int | Количество гостей |
| TotalPrice | decimal | Общая стоимость |
| Status | int enum | Статус бронирования |
| CreatedAt | datetime | Когда создано |

**Примеры:**

```
Id: 550e8400-e29b-41d4-a716-446655440100
RoomId: 550e8400-e29b-41d4-a716-446655440010
UserId: 550e8400-e29b-41d4-a716-446655440000
CheckInDate: 2025-04-14 14:00:00
CheckOutDate: 2025-04-16 10:00:00
GuestsCount: 2
TotalPrice: 300.00
Status: 1 (Confirmed)
CreatedAt: 2025-02-01 15:30:00
```

**Статусы:**
- 0 = Created (создано, ожидает подтверждения)
- 1 = Confirmed (подтверждено, оплачено)
- 2 = Cancelled (отменено)
- 3 = Completed (завершено, гость выехал)

---

### 5. RefreshTokens (Токены обновления)

Хранит информацию о Refresh Token для управления сессиями.

```sql
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Token NVARCHAR(MAX) NOT NULL UNIQUE,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_IsRevoked ON RefreshTokens(IsRevoked);
```

**Поля:**

| Поле | Тип | Описание |
|------|-----|---------|
| Id | GUID | Уникальный идентификатор |
| Token | string | Сам токен (base64) |
| UserId | GUID | Ссылка на пользователя |
| CreatedAt | datetime | Когда создан |
| ExpiresAt | datetime | Когда истекает (7 дней) |
| IsRevoked | bool | Был ли отозван (выход/обновление) |

**Примеры:**

```
Id: 550e8400-e29b-41d4-a716-446655440200
Token: base64_encoded_random_string_here
UserId: 550e8400-e29b-41d4-a716-446655440000
CreatedAt: 2025-02-01 10:30:00
ExpiresAt: 2025-02-08 10:30:00
IsRevoked: 0 (не отозван)

Id: 550e8400-e29b-41d4-a716-446655440201
Token: another_base64_token_here
UserId: 550e8400-e29b-41d4-a716-446655440000
CreatedAt: 2025-02-01 11:00:00
ExpiresAt: 2025-02-08 11:00:00
IsRevoked: 1 (отозван - пользователь вышел)
```

---

## 🔗 Связи между таблицами

### Users → RefreshTokens (1:N)

Один пользователь может иметь несколько активных Refresh Token (макс 5 сессий).

```
User (550e8400-...)
├─ RefreshToken #1 (создан 10:30, не отозван)
├─ RefreshToken #2 (создан 11:00, отозван)
├─ RefreshToken #3 (создан 12:30, не отозван)
└─ RefreshToken #4 (создан 14:00, не отозван)
```

### Users → Bookings (1:N)

Один пользователь может создать несколько бронирований.

```
User (john_doe)
├─ Booking #1 (14-16 апреля, номер 101)
├─ Booking #2 (20-22 апреля, номер 205)
└─ Booking #3 (01-05 мая, номер 101)
```

### RoomTypes → Rooms (1:N)

Один тип номера может быть у нескольких номеров.

```
RoomType (Стандартный номер)
├─ Room #101
├─ Room #102
├─ Room #103
└─ Room #104
```

### Rooms → Bookings (1:N)

Один номер может быть забронирован много раз (в разные даты).

```
Room #101
├─ Booking (14-16 апреля)
├─ Booking (20-22 апреля)
└─ Booking (01-05 мая)
```

---

## 📊 SQL запросы для анализа

### Получить свободные номера на конкретные даты

```sql
SELECT r.*, rt.*
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeId = rt.Id
WHERE r.Id NOT IN (
    SELECT DISTINCT RoomId
    FROM Bookings
    WHERE Status IN (0, 1)  -- Created или Confirmed
    AND CheckInDate < '2025-04-16'
    AND CheckOutDate > '2025-04-14'
)
AND rt.Id = '550e8400-...'  -- Ищем конкретный тип
ORDER BY r.Number;
```

### Получить статистику по бронированиям

```sql
SELECT 
    COUNT(*) as TotalBookings,
    COUNT(CASE WHEN Status = 1 THEN 1 END) as Confirmed,
    COUNT(CASE WHEN Status = 2 THEN 1 END) as Cancelled,
    COUNT(CASE WHEN Status = 3 THEN 1 END) as Completed,
    SUM(CASE WHEN Status = 1 THEN TotalPrice ELSE 0 END) as Revenue
FROM Bookings;
```

### Получить активные сессии пользователя

```sql
SELECT *
FROM RefreshTokens
WHERE UserId = '550e8400-...'
AND IsRevoked = 0
AND ExpiresAt > GETUTCDATE()
ORDER BY CreatedAt DESC;
```

### Найти номера на техническом обслуживании

```sql
SELECT r.*, rt.Name
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeId = rt.Id
WHERE r.Status = 2  -- Maintenance
ORDER BY r.Number;
```

---

## 🔒 Целостность данных

### Foreign Keys

- `Rooms.RoomTypeId` → `RoomTypes.Id` (ON DELETE RESTRICT)
  - Нельзя удалить тип если есть номера этого типа

- `Bookings.RoomId` → `Rooms.Id` (ON DELETE CASCADE)
  - При удалении номера удаляются все его бронирования

- `Bookings.UserId` → `Users.Id` (ON DELETE CASCADE)
  - При удалении пользователя удаляются все его бронирования

- `RefreshTokens.UserId` → `Users.Id` (ON DELETE CASCADE)
  - При удалении пользователя удаляются его токены

### Ограничения (Constraints)

```sql
-- Users
UNIQUE(Login)
UNIQUE(Email)
UNIQUE(PhoneNumber)

-- Rooms
UNIQUE(Number)

-- RefreshTokens
UNIQUE(Token)

-- Индексы для быстрого поиска
INDEX(Users.Login)
INDEX(Users.Email)
INDEX(Rooms.Status)
INDEX(Bookings.UserId)
INDEX(Bookings.Status)
INDEX(RefreshTokens.Token)
INDEX(RefreshTokens.IsRevoked)
```

---

## 📈 Размер данных

```
Примерные размеры при 1000 номеров и 10000 бронирований:

Users:              ~50-200 KB (зависит от текста в профиле)
RoomTypes:          ~5 KB
Rooms:              ~100 KB
Bookings:           ~1-2 MB
RefreshTokens:      ~300-500 KB (тестирование, затем очищаются)

ИТОГО: ~2-3 MB (очень маленько!)
```

---

## 🗑️ Очистка данных

### Удалить отозванные токены старше 1 месяца

```sql
DELETE FROM RefreshTokens
WHERE IsRevoked = 1
AND CreatedAt < DATEADD(MONTH, -1, GETUTCDATE());
```

### Удалить отменённые бронирования старше 6 месяцев

```sql
DELETE FROM Bookings
WHERE Status = 2  -- Cancelled
AND CreatedAt < DATEADD(MONTH, -6, GETUTCDATE());
```

### Удалить завершённые бронирования старше 1 года (архив)

```sql
DELETE FROM Bookings
WHERE Status = 3  -- Completed
AND CheckOutDate < DATEADD(YEAR, -1, GETUTCDATE());
```

---

**Последнее обновление:** Февраль 2025  
**Версия БД:** Entity Framework Core migrations
