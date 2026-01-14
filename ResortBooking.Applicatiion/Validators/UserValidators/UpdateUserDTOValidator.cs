using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.UserDTOs;

namespace ResortBooking.Application.Validators.UserValidators;

public class UpdateUserDTOValidator : AbstractValidator<UpdateUserDTO>
{
    public UpdateUserDTOValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email не должен быть пустым")
            .EmailAddress().WithMessage("Некорректный формат Email");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона не должен быть пустым")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Некорректный формат номера телефона");
    }
}
