using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Persistence.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Login).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.Login).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(13);
        builder.Property(x => x.PasswordHash).IsRequired();

        builder.Property(x => x.Role).IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
