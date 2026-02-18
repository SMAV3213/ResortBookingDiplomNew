using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.BookingsDTOs;

namespace ResortBooking.Application.Validators.BookingValidators;

/// <summary>
/// Валидатор для DTO создания бронирования.
/// 
/// FluentValidation - библиотека для декларативной валидации данных.
/// 
/// Как это работает:
/// 1. Когда контроллер получает CreateBookingDTO, ASP.NET автоматически запускает валидацию
/// 2. Если валидация не пройдена, возвращается 400 Bad Request с детальными ошибками
/// 3. Если валидация успешна, DTO передается в метод контроллера
/// 
/// Преимущества:
/// - Чистый, читаемый код (fluent interface)
/// - Можно создавать сложные правила с Cross-Property валидацией
/// - Сообщения об ошибках локализуются
/// 
/// Пример ответа при ошибке:
/// HTTP 400 Bad Request
/// {
///   "errors": {
///     "CheckIn": ["Дата заезда должна быть не раньше сегодняшнего дня"],
///     "GuestsCount": ["Количество гостей должно быть от 1 до 10"]
///   }
/// }
/// </summary>
public class CreateBookingDTOValidator : AbstractValidator<CreateBookingDTO>
{
    /// <summary>
    /// Конструктор определяет все правила валидации для CreateBookingDTO.
    /// 
    /// RuleFor(x => x.Property) - выбираем свойство для валидации
    /// Цепочка методов описывает правило:
    ///   .NotEmpty() - не пусто
    ///   .Must(predicate) - пользовательское правило
    ///   .InclusiveBetween(min, max) - между min и max (включительно)
    ///   .WithMessage("...") - сообщение об ошибке
    /// </summary>
    public CreateBookingDTOValidator()
    {
        // Правило 1: UserId обязателен (не может быть 0 или Guid.Empty)
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь обязателен");

        // Правило 2: RoomTypeId обязателен
        RuleFor(x => x.RoomTypeId)
            .NotEmpty().WithMessage("Тип комнаты обязателен");

        // Правило 3: Дата заезда не может быть в прошлом
        // d.Date >= DateTime.UtcNow.Date - сравниваем только даты, без времени
        // Must(predicate) - выполняем пользовательское условие
        RuleFor(x => x.CheckIn)
            .Must(d => d.Date >= DateTime.UtcNow.Date)
            .WithMessage("Дата заезда должна быть не раньше сегодняшнего дня");

        // Правило 4: Дата выезда должна быть позже даты заезда
        // (dto, checkOut) => checkOut.Date > dto.CheckIn.Date 
        // Это Cross-Property валидация: проверяем отношение между двумя свойствами
        // dto - весь объект CreateBookingDTO
        // checkOut - конкретное значение CheckOut (второй параметр из RuleFor)
        RuleFor(x => x.CheckOut)
            .Must((dto, checkOut) => checkOut.Date > dto.CheckIn.Date)
            .WithMessage("Дата выезда должна быть позже даты заезда");

        // Правило 5: Количество гостей разумное (от 1 до 10)
        // InclusiveBetween = включая границы (1 и 10 допустимы)
        // ExclusiveBetween был бы без границ (1 и 10 недопустимы)
        RuleFor(x => x.GuestsCount)
            .InclusiveBetween(1, 10)
            .WithMessage("Количество гостей должно быть от 1 до 10");
    }
}