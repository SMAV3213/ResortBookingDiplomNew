using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Persistence.Configurations;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Capacity).IsRequired();
        builder.Property(x => x.PricePerNight)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.HasMany(x => x.Images)
            .WithOne(x => x.RoomType)
            .HasForeignKey(x => x.RoomTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
