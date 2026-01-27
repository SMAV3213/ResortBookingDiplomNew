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

    public async Task<ApiResponse<PagedResult<UserDTO>>> GetAllAsync(UsersQueryDTO query)
    {
        var paged = await _repository.SearchAsync(query);

        var items = paged.Items.Select(Map).ToList();

        return ApiResponse<PagedResult<UserDTO>>.Ok(new PagedResult<UserDTO>
        {
            Items = items,
            Total = paged.Total,
            Page = paged.Page,
            PageSize = paged.PageSize
        }, "Пользователи получены");
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

        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingByEmail = await _repository.GetByEmailAsync(dto.Email);
            if (existingByEmail != null && existingByEmail.Id != user.Id)
                return ApiResponse<bool>.Fail("Пользователь с такой почтой уже существует");
        }

        if (!string.Equals(user.PhoneNumber, dto.PhoneNumber, StringComparison.OrdinalIgnoreCase))
        {
            var existingByPhone = await _repository.GetByPhoneAsync(dto.PhoneNumber);
            if (existingByPhone != null && existingByPhone.Id != user.Id)
                return ApiResponse<bool>.Fail("Пользователь с таким номером телефона уже существует");
        }

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
