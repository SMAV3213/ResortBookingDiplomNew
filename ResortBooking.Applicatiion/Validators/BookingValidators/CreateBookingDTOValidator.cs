using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.BookingsDTOs;

namespace ResortBooking.Application.Validators.BookingValidators;

public class CreateBookingDTOValidator : AbstractValidator<CreateBookingDTO>
{
    public CreateBookingDTOValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь обязателен");

        RuleFor(x => x.RoomTypeId)
            .NotEmpty().WithMessage("Тип комнаты обязателен");

        RuleFor(x => x.CheckIn)
            .Must(d => d.Date >= DateTime.UtcNow.Date)
            .WithMessage("Дата заезда должна быть не раньше сегодняшнего дня");

        RuleFor(x => x.CheckOut)
            .Must((dto, checkOut) => checkOut.Date > dto.CheckIn.Date)
            .WithMessage("Дата выезда должна быть позже даты заезда");

        RuleFor(x => x.GuestsCount)
            .InclusiveBetween(1, 10)
            .WithMessage("Количество гостей должно быть от 1 до 10");
    }
}