using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.DTOs;

public static class UserDTOs
{
    public record UserDTO
    (
        Guid Id,
        string Login,
        string Email,
        string PhoneNumber,
        string Role
    );

    public record UpdateUserDTO
    (
        string Email,
        string PhoneNumber
    );

    public record ChangeUserRoleDTO
    (
        UserRole Role
    );

    public record UsersQueryDTO(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,     
    string? Role = null,        
    string SortBy = "login",
    string SortDir = "asc" 
);
}
