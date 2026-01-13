using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalPrice)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Room)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.RoomId);
    }
}
