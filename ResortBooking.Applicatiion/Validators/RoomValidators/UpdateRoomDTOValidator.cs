using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomDTOs;

namespace ResortBooking.Application.Validators.RoomValidators;

public class UpdateRoomDTOValidator : AbstractValidator<UpdateRoomDTO>
{
    public UpdateRoomDTOValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Номер комнаты обязателен")
            .MaximumLength(10).WithMessage("Номер  комнаты не должен превышать 10 символов");
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Статус комнаты обязателен")
            .IsInEnum().WithMessage("Недопустимый статус комнаты");
        RuleFor(x => x.RoomTypeId)
            .NotEmpty().WithMessage("Id типа комнаты обязателен");
    }
}
