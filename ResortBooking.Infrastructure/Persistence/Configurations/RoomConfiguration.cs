using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.RoomType)
            .WithMany(x => x.Rooms)
            .HasForeignKey(x => x.RoomTypeId);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
