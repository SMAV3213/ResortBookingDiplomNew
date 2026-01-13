using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomDTOs;

namespace ResortBooking.Application.Validators.RoomValidators;

public class CreateRoomDTOValidator : AbstractValidator<CreateRoomDTO>
{
    public CreateRoomDTOValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Номер комнаты обязателен")
            .MaximumLength(10).WithMessage("Номер  комнаты не должен превышать 10 символов");
        RuleFor(x => x.RoomTypeId)
            .NotEmpty().WithMessage("Id типа комнаты обязателен");
    }
}
