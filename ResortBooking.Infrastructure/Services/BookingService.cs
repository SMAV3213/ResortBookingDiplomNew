using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.BookingsDTOs;

namespace ResortBooking.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IRoomTypeRepository roomTypeRepository)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _roomTypeRepository = roomTypeRepository;
    }

    public async Task<ApiResponse<List<BookingDTO>>> GetAllAsync()
    {
        var bookings = await _bookingRepository.GetAllAsync();
        var result = bookings.Select(b => new BookingDTO(
            b.Id,
            b.RoomId,
            b.UserId,
            b.CheckInDate,
            b.CheckOutDate,
            b.GuestsCount,
            b.TotalPrice,
            b.Status.ToString()
        )).ToList();

        return ApiResponse<List<BookingDTO>>.Ok(result, "Брони успешно получены");
    }

    public async Task<ApiResponse<BookingDTO>> GetByIdAsync(Guid id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
            return ApiResponse<BookingDTO>.Fail("Бронь не найдена");

        return ApiResponse<BookingDTO>.Ok(new BookingDTO(
            booking.Id,
            booking.RoomId,
            booking.UserId,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.GuestsCount,
            booking.TotalPrice,
            booking.Status.ToString()
        ), "Бронь успешно получена");
    }

    public async Task<ApiResponse<List<BookingDTO>>> GetByUserIdAsync(Guid userId)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(userId);
        var result = bookings.Select(b => new BookingDTO(
            b.Id,
            b.RoomId,
            b.UserId,
            b.CheckInDate,
            b.CheckOutDate,
            b.GuestsCount,
            b.TotalPrice,
            b.Status.ToString()
        )).ToList();

        return ApiResponse<List<BookingDTO>>.Ok(result, "Брони пользователя успешно получены");
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreateBookingDTO dto)
    {
        var rooms = (await _roomRepository.GetAllAsync())
            .Where(r => r.RoomTypeId == dto.RoomTypeId && r.Status != RoomStatus.Maintenance)
            .ToList();

        if (!rooms.Any())
            return ApiResponse<Guid>.Fail("Нет доступных комнат данного типа");

        var bookingsAll = await _bookingRepository.GetAllAsync();

        var freeRoom = rooms.FirstOrDefault(room =>
        {
            var bookings = bookingsAll
                .Where(b => b.RoomId == room.Id && b.Status == BookingStatus.Confirmed)
                .ToList();

            return !bookings.Any(b => dto.CheckIn < b.CheckOutDate && dto.CheckOut > b.CheckInDate);
        });

        if (freeRoom == null)
            return ApiResponse<Guid>.Fail("Нет свободных комнат на выбранные даты");

        var roomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
        if (roomType == null)
            return ApiResponse<Guid>.Fail("Тип комнаты не найден");

        var nights = (dto.CheckOut - dto.CheckIn).Days;
        var totalPrice = roomType.PricePerNight * nights;

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = freeRoom.Id,
            UserId = dto.UserId,
            CheckInDate = dto.CheckIn,
            CheckOutDate = dto.CheckOut,
            GuestsCount = dto.GuestsCount,
            TotalPrice = totalPrice,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        return ApiResponse<Guid>.Ok(booking.Id, "Бронь успешно создана");
    }

    public async Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateBookingDTO dto)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
            return ApiResponse<bool>.Fail("Бронь не найдена");

        booking.CheckInDate = dto.CheckIn;
        booking.CheckOutDate = dto.CheckOut;
        booking.GuestsCount = dto.GuestsCount;


        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Бронь успешно обновлена");
    }

    public async Task<ApiResponse<bool>> CancelAsync(Guid id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
            return ApiResponse<bool>.Fail("Бронь не найдена");

        var timeUntilCheckIn = booking.CheckInDate.ToUniversalTime() - DateTime.UtcNow;

        if (timeUntilCheckIn.TotalDays < 3)
            return ApiResponse<bool>.Fail("Нельзя отменить бронь, если до заезда осталось менее 3 дней");

        booking.Status = BookingStatus.Cancelled;

        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Бронь успешно отменена");
    }
}
