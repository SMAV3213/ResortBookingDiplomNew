using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.AuthDTOs;

namespace ResortBooking.Application.Validators.Auth;

public class RegisterUserDTOValidator : AbstractValidator<RegisterUserDTO>
{
    public RegisterUserDTOValidator()
    {
        RuleFor(x => x.Login)
           .NotEmpty().WithMessage("Логин обязателен")
           .MinimumLength(4).WithMessage("Логин должен содержать минимум 4 символа");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Почта обязательна")
            .EmailAddress().WithMessage("Некорректный формат почты");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона обязателен");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(8).WithMessage("Пароль должен быть не короче 8 символов")
            .Matches("[A-Z]").WithMessage("Пароль должен содержать заглавную букву")
            .Matches("[0-9]").WithMessage("Пароль должен содержать цифру");
    }
}
