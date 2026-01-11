using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entities;

public class User : BaseEntity
{
    public string Login { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public UserRole Role { get; set; } = UserRole.User;

    // Навигация
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
