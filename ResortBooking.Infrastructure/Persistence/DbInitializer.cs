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

        await SeedAdminAsync(context);

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
