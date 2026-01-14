using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.UserDTOs;

namespace ResortBooking.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<UserDTO>>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();

        var result = users.Select(Map).ToList();

        return ApiResponse<List<UserDTO>>.Ok(result, "Пользователи получены");
    }

    public async Task<ApiResponse<UserDTO>> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            return ApiResponse<UserDTO>.Fail("Пользователь не найден");

        return ApiResponse<UserDTO>.Ok(Map(user), "Пользователь получен");
    }

    public async Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateUserDTO dto)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            return ApiResponse<bool>.Fail("Пользователь не найден");

        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;

        await _repository.UpdateAsync(user);
        await _repository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Данные пользователя обновлены");
    }

    public async Task<ApiResponse<bool>> ChangeRoleAsync(Guid id, ChangeUserRoleDTO dto)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            return ApiResponse<bool>.Fail("Пользователь не найден");

        user.Role = dto.Role;

        await _repository.UpdateAsync(user);
        await _repository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Роль пользователя изменена");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            return ApiResponse<bool>.Fail("Пользователь не найден");

        await _repository.RemoveAsync(id);
        await _repository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Пользователь удалён");
    }

    private static UserDTO Map(User user) =>
       new(
           user.Id,
           user.Login,
           user.Email,
           user.PhoneNumber,
           user.Role.ToString()
       );
}
