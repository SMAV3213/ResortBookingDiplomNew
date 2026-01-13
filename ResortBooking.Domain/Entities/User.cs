using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entites;

public class User
{
    public Guid Id { get; set; }

    public string Login { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
