using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;
using ResortBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.UserDTOs;

namespace ResortBooking.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<User>> SearchAsync(UsersQueryDTO q, CancellationToken ct = default)
    {
        var page = q.Page < 1 ? 1 : q.Page;

        var pageSize = q.PageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => q.PageSize
        };

        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            // Для SQL Server с CI collation Contains обычно уже case-insensitive
            query = query.Where(u =>
                u.Login.Contains(s) ||
                u.Email.Contains(s) ||
                u.PhoneNumber.Contains(s)
            );
        }

        if (!string.IsNullOrWhiteSpace(q.Role) &&
            Enum.TryParse<UserRole>(q.Role, ignoreCase: true, out var role))
        {
            query = query.Where(u => u.Role == role);
        }

        var desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = (q.SortBy?.ToLowerInvariant(), desc) switch
        {
            ("email", true) => query.OrderByDescending(x => x.Email).ThenByDescending(x => x.Id),
            ("email", false) => query.OrderBy(x => x.Email).ThenBy(x => x.Id),

            ("role", true) => query.OrderByDescending(x => x.Role).ThenByDescending(x => x.Id),
            ("role", false) => query.OrderBy(x => x.Role).ThenBy(x => x.Id),

            ("login", true) => query.OrderByDescending(x => x.Login).ThenByDescending(x => x.Id),
            _ => query.OrderBy(x => x.Login).ThenBy(x => x.Id),
        };

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<User>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<User?> GetByLoginAsync(string login)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Login == login);
    }
    public Task<User?> GetByEmailAsync(string email)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    public Task<User?> GetByPhoneAsync(string phoneNumber)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }
    public Task<User?> GetByIdAsync(Guid id)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.AsNoTracking().ToListAsync();
    }
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }
    public async Task RemoveAsync(Guid id) {
        var user = await _context.Users.FindAsync(id);
        if (user != null) {
            _context.Users.Remove(user);
        }
    }
    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
