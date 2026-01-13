using Microsoft.EntityFrameworkCore;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ResortBooking.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<RoomTypeImage> RoomTypeImages => Set<RoomTypeImage>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
