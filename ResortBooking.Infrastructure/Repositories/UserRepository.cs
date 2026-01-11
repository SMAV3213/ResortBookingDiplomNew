using Microsoft.EntityFrameworkCore;
using ResortBooking.Application.Interfaces;
using ResortBooking.Domain.Entities;
using ResortBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ResortBookingDbContext _db;

    public UserRepository(ResortBookingDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
