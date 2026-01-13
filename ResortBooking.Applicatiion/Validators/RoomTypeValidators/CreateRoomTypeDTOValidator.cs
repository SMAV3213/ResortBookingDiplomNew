using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.Application.Validators.RoomTypeValidators;

public class CreateRoomTypeDTOValidator : AbstractValidator<CreateRoomTypeDTO>
{
    public CreateRoomTypeDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название типа комнаты обязательно")
            .MaximumLength(100).WithMessage("Название типа комнаты не должно превышать 100 символов");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание типа комнаты обязательно")
            .MaximumLength(1000).WithMessage("Описание типа комнаты не должно превышать 1000 символов");
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Максимальная вместимость должна быть больше 0");
        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("Цена за ночь должна быть больше 0");
    }
}
