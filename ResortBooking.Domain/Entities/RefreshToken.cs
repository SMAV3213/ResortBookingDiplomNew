using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entites;

public class RefreshToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
