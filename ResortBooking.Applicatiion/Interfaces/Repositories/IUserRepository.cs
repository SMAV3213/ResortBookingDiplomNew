using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneAsync(string phoneNumber);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task RemoveAsync(Guid Id);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
}
