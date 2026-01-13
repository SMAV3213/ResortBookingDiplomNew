using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.BookingsDTOs;

namespace ResortBooking.Application.Validators;

public class UpdateBookingDTOValidator : AbstractValidator<UpdateBookingDTO>
{
    public UpdateBookingDTOValidator()
    {
        RuleFor(x => x.CheckIn)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Дата заезда должна быть не раньше сегодняшнего дня");

        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn)
            .WithMessage("Дата выезда должна быть позже даты заезда");

        RuleFor(x => x.GuestsCount)
            .GreaterThan(0).WithMessage("Количество гостей должно быть больше 0");
    }
}
