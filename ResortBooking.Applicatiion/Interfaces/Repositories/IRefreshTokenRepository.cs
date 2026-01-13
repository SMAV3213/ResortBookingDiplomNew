using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetAsync(string token);

    Task<int> CountActiveByUserIdAsync(Guid userId);
    Task DeleteOldestByUserIdAsync(Guid userId, int count);

    Task SaveChangesAsync();
}
