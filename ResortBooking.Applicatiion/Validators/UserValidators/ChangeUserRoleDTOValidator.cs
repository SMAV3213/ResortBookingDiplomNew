using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.UserDTOs;

namespace ResortBooking.Application.Validators.UserValidators;

public class ChangeUserRoleDTOValidator : AbstractValidator<ChangeUserRoleDTO>
{
    public ChangeUserRoleDTOValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Некорректная роль пользователя");
    }
}
