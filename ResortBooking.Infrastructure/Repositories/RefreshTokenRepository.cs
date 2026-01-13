using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Domain.Entites;
using ResortBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;
    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
    }
    public Task<RefreshToken?> GetAsync(string token)
    {
        return _context.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked);
    }
    public async Task<int> CountActiveByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .CountAsync(x =>
                x.UserId == userId &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow);
    }

    public async Task DeleteOldestByUserIdAsync(Guid userId, int count)
    {
        var tokensToDelete = await _context.RefreshTokens
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(tokensToDelete);
        await _context.SaveChangesAsync();
    }
    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
