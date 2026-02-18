using Microsoft.EntityFrameworkCore;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ResortBooking.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext - "воротца" к базе данных.
/// 
/// DbContext отвечает за:
/// - Управление подключением к БД
/// - Отслеживание изменений объектов
/// - Выполнение CRUD операций (Create, Read, Update, Delete)
/// - Работа с миграциями
/// 
/// Жизненный цикл: Scoped (один экземпляр на HTTP request)
/// Это безопасно, потому что каждый запрос получает свой контекст.
/// 
/// Важно: DbContext должен быть Scoped, а не Singleton!
/// Иначе будут проблемы с многопоточностью и утечки памяти.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Конструктор принимает DbContextOptions из DI контейнера.
    /// 
    /// Где используется:
    /// services.AddDbContext<ApplicationDbContext>(options =>
    ///     options.UseSqlServer(connectionString)
    /// );
    /// 
    /// На основе этих опций создается подключение к SQL Server.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet - это коллекция всех записей таблицы.
    /// 
    /// Примеры использования:
    ///   var user = await Users.FindAsync(id);           // SELECT по ID
    ///   var allUsers = await Users.ToListAsync();        // SELECT всех
    ///   Users.Add(newUser);                              // INSERT
    ///   Users.Update(user);                              // UPDATE
    ///   Users.Remove(user);                              // DELETE
    ///   await SaveChangesAsync();                        // COMMIT в БД
    /// 
    /// Property Name (Users, Rooms) = название DbSet
    /// Generic Type (User, Room) = Entity класс (из Domain слоя)
    /// 
    /// Set<T>() - возвращает DbSet для типа T
    /// </summary>

    // 👥 Users - таблица пользователей (email, пароль, роль)
    public DbSet<User> Users => Set<User>();

    // 🏢 RoomTypes - типы номеров (люкс, стандарт и т.д.)
    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    // 🖼️ RoomTypeImages - фото для каждого типа номера
    public DbSet<RoomTypeImage> RoomTypeImages => Set<RoomTypeImage>();

    // 🛏️ Rooms - конкретные номера в отеле
    public DbSet<Room> Rooms => Set<Room>();

    // 📅 Bookings - бронирования номеров
    public DbSet<Booking> Bookings => Set<Booking>();

    // 🔑 RefreshTokens - токены для обновления сессии
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// OnModelCreating - настройка конфигурации моделей (EF Fluent API).
    /// 
    /// Здесь мы задаём правила для каждой таблицы:
    /// - Первичные ключи (PK)
    /// - Иностранные ключи (FK)
    /// - Ограничения (constraints)
    /// - Индексы (indexes) для быстрого поиска
    /// - Значения по умолчанию
    /// 
    /// Две способа конфигурации:
    /// 1. Data Annotations (атрибуты над properties) - просто, но ограничено
    /// 2. Fluent API (OnModelCreating) - мощнее, более гибко
    /// 
    /// Мы используем IEntityTypeConfiguration<T> для каждой сущности.
    /// Конфигурации находятся в отдельных файлах (User.cs, Room.cs и т.д.)
    /// Это делает код чище и организованнее.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ApplyConfigurationsFromAssembly ищет все классы, реализующие IEntityTypeConfiguration<T>
        // и применяет их конфигурацию к моделям.
        // 
        // Примеры конфигураций:
        // - UserConfiguration.cs - настройка таблицы Users
        // - RoomConfiguration.cs - настройка таблицы Rooms
        // - BookingConfiguration.cs - настройка таблицы Bookings
        // 
        // Assembly.GetExecutingAssembly() ищет в текущей сборке (ResortBooking.Infrastructure)
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Вызываем базовый метод родителя
        // Это может быть важно для некоторых опций
        base.OnModelCreating(modelBuilder);
    }
}
