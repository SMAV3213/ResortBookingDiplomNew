using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.AuthDTOs;

namespace ResortBooking.Application.Validators.Auth;

public class LoginDTOValidator : AbstractValidator<LoginDTO>
{
    public LoginDTOValidator()
    {
        RuleFor(x => x.Login)
           .NotEmpty().WithMessage("Логин обязателен");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}
