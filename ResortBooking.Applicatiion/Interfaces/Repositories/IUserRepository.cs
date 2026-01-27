using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.UserDTOs;

namespace ResortBooking.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<PagedResult<User>> SearchAsync(UsersQueryDTO query, CancellationToken ct = default);
    Task<User?> GetByLoginAsync(string login);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneAsync(string phoneNumber);
    Task<User?> GetByIdAsync(Guid id);

    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);

    Task RemoveAsync(Guid Id);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
}
