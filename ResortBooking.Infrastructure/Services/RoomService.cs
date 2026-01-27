using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.RoomDTOs;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.Infrastructure.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _repository;

    public RoomService(IRoomRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<PagedResult<RoomDTO>>> GetAllAsync(RoomsQueryDTO query)
    {
        var paged = await _repository.SearchAsync(query);

        var items = paged.Items.Select(x => new RoomDTO(
            x.Id,
            x.Number,
            x.Status.ToString(),
            new RoomTypeInRoomsDTO(
                x.RoomType.Id,
                x.RoomType.Name,
                x.RoomType.Description,
                x.RoomType.Capacity,
                x.RoomType.PricePerNight
            )
        )).ToList();

        return ApiResponse<PagedResult<RoomDTO>>.Ok(new PagedResult<RoomDTO>
        {
            Items = items,
            Total = paged.Total,
            Page = paged.Page,
            PageSize = paged.PageSize
        }, "Комнаты успешно получены");
    }

    public async Task<ApiResponse<RoomDTO>> GetByIdAsync(Guid id)
    {
        var room = await _repository.GetByIdAsync(id);
        if (room == null)
            return ApiResponse<RoomDTO>.Fail("Комната не найдена");

        var dto = new RoomDTO(
            room.Id,
            room.Number,
            room.Status.ToString(),
            new RoomTypeInRoomsDTO(
                room.RoomType.Id,
                room.RoomType.Name,
                room.RoomType.Description,
                room.RoomType.Capacity,
                room.RoomType.PricePerNight
            )
        );

        return ApiResponse<RoomDTO>.Ok(dto, "Комната получена");
    }


    public async Task<ApiResponse<Guid>> AddAsync(CreateRoomDTO dto)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Number = dto.Number,
            RoomTypeId = dto.RoomTypeId,
            Status = RoomStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        return ApiResponse<Guid>.Ok(
            room.Id,
            "Комната успешно создана");
    }

    public async Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateRoomDTO dto)
    {
        var room = await _repository.GetByIdAsync(id);
        if (room == null)
            return ApiResponse<bool>.Fail("Комната не найдена");

        room.Number = dto.Number;
        room.RoomTypeId = dto.RoomTypeId;

        if (!Enum.TryParse<RoomStatus>(dto.Status, out var status))
            return ApiResponse<bool>.Fail("Некорректный статус комнаты");

        room.Status = status;

        _repository.Update(room);
        await _repository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Комната успешно обновлена");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var room = await _repository.GetByIdAsync(id);
        if (room == null)
            return ApiResponse<bool>.Fail("Комната не найдена");

        _repository.Delete(room);
        await _repository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(
            true,
            "Комната успешно удалена");
    }
    public async Task<ApiResponse<List<RoomDTO>>> GetAvailableRoomsAsync(Guid roomTypeId, DateTime checkIn, DateTime checkOut)
    {
        var rooms = await _repository.GetAvailableByRoomTypeAsync(roomTypeId);
        var result = rooms.Select(x => new RoomDTO(
            x.Id,
            x.Number,
            x.Status.ToString(),
            new RoomTypeInRoomsDTO(
                x.RoomType.Id,
                x.RoomType.Name,
                x.RoomType.Description,
                x.RoomType.Capacity,
                x.RoomType.PricePerNight
            )
        )).ToList();
        return ApiResponse<List<RoomDTO>>.Ok(
            result,
            "Доступные комнаты успешно получены");
    }
}

