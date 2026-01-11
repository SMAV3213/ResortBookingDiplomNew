using ResortBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
