using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.Application.Validators.RoomTypeValidators;

public class UpdateRoomTypeDTOValidator : AbstractValidator<UpdateRoomTypeDTO>
{
    public UpdateRoomTypeDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название типа комнаты обязательно")
            .MaximumLength(100).WithMessage("Название типа комнаты не должно превышать 100 символов");
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Описание типа комнаты не должно превышать 500 символов");
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Максимальная вместимость должна быть больше 0");
        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("Цена за ночь должна быть больше 0");
    }
}
