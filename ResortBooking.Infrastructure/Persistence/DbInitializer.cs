using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;

namespace ResortBooking.Infrastructure.Persistence;

/// <summary>
/// Инициализатор базы данных - создаёт начальные данные при первом запуске
/// </summary>
public static class DbInitializer
{
    private static readonly PasswordHasher<User> _passwordHasher = new();

    /// <summary>
    /// Инициализирует БД и создаёт админов если их нет
    /// </summary>
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Применяем миграции
        await context.Database.MigrateAsync();

        await SeedUsersAsync(context);

        await SeedAdminAsync(context);

        await SeedRoomTypesAsync(context);

        await SeedRoomsAsync(context);

        await SeedBookingsAsync(context);
    }

    private static async Task SeedRoomTypesAsync(ApplicationDbContext context)
    {
        if (await context.RoomTypes.AnyAsync()) return;

        var roomTypes = new List<RoomType>
        {
            new() { Id = Guid.Parse("cb220aad-e08f-4ce1-860e-6f992cbd12eb"), Name = "Family", Description = "Семейный номер, максимальная вместимость.", Capacity = 5, PricePerNight = 13000.00 },
            new() { Id = Guid.Parse("22aa048e-9d22-4c1f-9ed6-a4243e2a90f5"), Name = "Standard", Description = "Стандартный номер для двоих, уютный и тихий.", Capacity = 2, PricePerNight = 4200.00 },
            new() { Id = Guid.Parse("1c2ec2d6-bb55-4021-8394-ad3a0187bd73"), Name = "Suite", Description = "Люкс с зоной отдыха, подойдёт для семьи.", Capacity = 4, PricePerNight = 11000.00},
            new() { Id = Guid.Parse("1b908c21-01a7-4b7f-9ef5-c23e0e9d7d93"), Name = "Deluxe", Description = "Улучшенный номер, вид и расширенные удобства.", Capacity = 2, PricePerNight = 7600.00 },
            new() { Id = Guid.Parse("a281ac3a-bb83-4e56-8a36-d3d345789042"), Name = "Comfort", Description = "Больше пространства, удобен для пары или семьи с ребёнком.", Capacity = 3, PricePerNight = 5900.00 }
        };

        context.RoomTypes.AddRange(roomTypes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRoomsAsync(ApplicationDbContext context)
    {
        if (await context.Rooms.AnyAsync()) return;

        var rooms = new List<Room>
        {
            new() { Id = Guid.Parse("f489e35a-90bb-4323-a2ec-0650f312abcb"), Number = "201", Status = RoomStatus.Available, RoomTypeId = Guid.Parse("1b908c21-01a7-4b7f-9ef5-c23e0e9d7d93"), CreatedAt = DateTime.Parse("2026-02-01T12:22:55") },
            new() { Id = Guid.Parse("b6fb30cd-246d-4ad7-8165-1725b72acf92"), Number = "202", Status = RoomStatus.Available, RoomTypeId = Guid.Parse("1c2ec2d6-bb55-4021-8394-ad3a0187bd73"), CreatedAt = DateTime.Parse("2026-02-01T12:23:00") },
            new() { Id = Guid.Parse("5322343e-6e8e-4b37-b281-23a4c8d6afe8"), Number = "102", Status = RoomStatus.Available, RoomTypeId = Guid.Parse("a281ac3a-bb83-4e56-8a36-d3d345789042"), CreatedAt = DateTime.Parse("2026-02-01T12:22:49") },
            new() { Id = Guid.Parse("9492c1fa-8f6f-410b-ae15-6b7a7094da08"), Number = "301", Status = RoomStatus.Available, RoomTypeId = Guid.Parse("cb220aad-e08f-4ce1-860e-6f992cbd12eb"), CreatedAt = DateTime.Parse("2026-02-01T12:23:06") },
            new() { Id = Guid.Parse("45e1668b-731c-4a2c-9052-c5bc3874de8c"), Number = "101", Status = RoomStatus.Available, RoomTypeId = Guid.Parse("22aa048e-9d22-4c1f-9ed6-a4243e2a90f5"), CreatedAt = DateTime.Parse("2026-02-01T12:22:43") }
        };

        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var users = new List<User>
        {
            new() { Id = Guid.Parse("6b591b36-1e20-4a97-845e-48fa29f553ba"), Login = "SMAv12", PhoneNumber = "8999999999", Email = "SMAv1@aaa.a", PasswordHash = "AQAAAAIAAYagAAAAECWBC34owkzKXUnhJLXdrtZnWc7eiyH5gs5CS3B+yY4Up8h//oB2gw5oGZY/3Tcw4g==", Role = UserRole.User, CreatedAt = DateTime.Parse("2026-02-01T10:06:41") },
            new() { Id = Guid.Parse("4ed2932b-e7df-48e7-a842-a8727890b3c1"), Login = "SMAv3", PhoneNumber = "+79016500752", Email = "SMAv1@gmail.com", PasswordHash = "AQAAAAIAAYagAAAAEDkHf5+030Ox/jc7iwkQaUhIB8Ju7ammnSW7oxwwx36D1smwaibsBW5BFDjgIGL5Uw==", Role = UserRole.User, CreatedAt = DateTime.Parse("2026-02-18T04:16:20") },
            new() { Id = Guid.Parse("35656d16-837a-4cbd-8581-ea3562f55b2e"), Login = "SMAv1", PhoneNumber = "222", Email = "evdokimovvlad650@gmail.com", PasswordHash = "AQAAAAIAAYagAAAAENyZiafyRgk0z2yqgLJa7ZFoXjAS3m59Qcsghyp/VXBgjR4+fqOKBAGlQi4/A7yETQ==", Role = UserRole.Admin, CreatedAt = DateTime.Parse("2026-01-12T13:18:57") }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBookingsAsync(ApplicationDbContext context)
    {
        if (await context.Bookings.AnyAsync()) return;

        var bookings = new List<Booking>
        {
            new() { Id = Guid.Parse("295b2a30-7e67-4f78-8399-32cc9a768287"), CheckInDate = DateTime.Parse("2026-02-01T12:26:41"), CheckOutDate = DateTime.Parse("2026-02-03T12:26:41"), TotalPrice = 8400.00, Status = BookingStatus.Confirmed, UserId = Guid.Parse("35656d16-837a-4cbd-8581-ea3562f55b2e"), RoomId = Guid.Parse("45e1668b-731c-4a2c-9052-c5bc3874de8c"), CreatedAt = DateTime.Parse("2026-02-01T12:27:44"), GuestsCount = 3 },
            new() { Id = Guid.Parse("67bd74a9-d892-4dd3-bbcc-66d622acf408"), CheckInDate = DateTime.Parse("2026-02-18"), CheckOutDate = DateTime.Parse("2026-02-19"), TotalPrice = 11000.00, Status = BookingStatus.Confirmed, UserId = Guid.Parse("4ed2932b-e7df-48e7-a842-a8727890b3c1"), RoomId = Guid.Parse("b6fb30cd-246d-4ad7-8165-1725b72acf92"), CreatedAt = DateTime.Parse("2026-02-18T04:16:53"), GuestsCount = 2 },
            new() { Id = Guid.Parse("b4ac5aea-7f7f-4988-a7d5-c72a4a23e440"), CheckInDate = DateTime.Parse("2026-02-19"), CheckOutDate = DateTime.Parse("2026-02-21"), TotalPrice = 26000.00, Status = BookingStatus.Confirmed, UserId = Guid.Parse("35656d16-837a-4cbd-8581-ea3562f55b2e"), RoomId = Guid.Parse("9492c1fa-8f6f-410b-ae15-6b7a7094da08"), CreatedAt = DateTime.Parse("2026-02-18T09:41:49"), GuestsCount = 2 }
        };

        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Создаёт административного пользователя если его ещё нет
    /// </summary>
    private static async Task SeedAdminAsync(ApplicationDbContext context)
    {
        // Проверяем есть ли уже админ в БД
        var adminExists = await context.Users.AnyAsync(u => u.Role == UserRole.Admin);
        
        if (adminExists)
        {
            Console.WriteLine("✅ Админ уже существует в БД.");
            return;
        }

        // Создаём нового админа
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Login = "admin",
            Email = "admin@resort-booking.com",
            PhoneNumber = "+79999999999",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        // Хешируем пароль используя тот же алгоритм что и AuthService
        adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "AdminPassword@111");

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Администратор успешно создан.");
        Console.WriteLine("   Login: admin");
        Console.WriteLine("   Password: AdminPassword@111");
        Console.WriteLine("   Email: admin@resort-booking.com");
    }
}
