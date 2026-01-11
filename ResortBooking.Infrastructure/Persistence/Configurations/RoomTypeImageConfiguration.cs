using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResortBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Persistence.Configurations;

public class RoomTypeImageConfiguration : IEntityTypeConfiguration<RoomTypeImage>
{
    public void Configure(EntityTypeBuilder<RoomTypeImage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
            .IsRequired();

        builder.HasOne(x => x.RoomType)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.RoomTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
