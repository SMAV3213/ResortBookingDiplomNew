using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Auth;

public class JwtSettings
{
    public string Secret { get; set; } = null!;
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
