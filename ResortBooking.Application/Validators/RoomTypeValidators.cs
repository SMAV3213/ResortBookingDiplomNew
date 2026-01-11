using ResortBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
namespace ResortBooking.Application.Validators;


public class RoomTypeValidators
{
    public class CreateValidator : AbstractValidator<RoomTypeDTOs.Create>
    {
        public CreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название типа номера не может быть пустым")
                .MaximumLength(100).WithMessage("Название не может превышать 100 символов");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Описание типа номера не может быть пустым");

            RuleFor(x => x.PricePerNight)
                .GreaterThanOrEqualTo(0).WithMessage("Цена за ночь не может быть отрицательной");

            RuleFor(x => x.MaxGuests)
                .GreaterThan(0).WithMessage("Количество гостей должно быть больше 0");

            RuleForEach(x => x.ImagesUrls)
                .NotEmpty().WithMessage("URL фотографии не может быть пустым")
                .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                .WithMessage("Неверный формат URL фотографии");
        }
    }

    public class UpdateValidator : AbstractValidator<RoomTypeDTOs.Update>
    {
        public UpdateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название типа номера не может быть пустым")
                .MaximumLength(100).WithMessage("Название не может превышать 100 символов");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Описание типа номера не может быть пустым");

            RuleFor(x => x.PricePerNight)
                .GreaterThanOrEqualTo(0).WithMessage("Цена за ночь не может быть отрицательной");

            RuleFor(x => x.MaxGuests)
                .GreaterThan(0).WithMessage("Количество гостей должно быть больше 0");

            RuleForEach(x => x.ImagesUrls)
                .NotEmpty().WithMessage("URL фотографии не может быть пустым")
                .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                .WithMessage("Неверный формат URL фотографии");
        }
    }
}
