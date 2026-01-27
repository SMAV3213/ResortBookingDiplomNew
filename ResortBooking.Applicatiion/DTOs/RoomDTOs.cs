using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.DTOs;

public class RoomDTOs
{
    public record RoomDTO(
        Guid Id,
        string Number,
        string Status,
        RoomTypeInRoomsDTO RoomType
    );

    public record CreateRoomDTO(
        string Number,
        Guid RoomTypeId
    );

    public record UpdateRoomDTO(
        string Number,
        string Status,
        Guid RoomTypeId
    );

    public record RoomTypeInRoomsDTO(
        Guid Id,
        string Name,
        string Description,
        int Capacity,
        decimal PricePerNight
    );
    public record RoomsQueryDTO(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,      // поиск по номеру комнаты
    Guid? RoomTypeId = null,    // фильтр по типу
    string? Status = null,      // Available/Occupied/Maintenance
    string SortBy = "number",   // number/status/createdAt
    string SortDir = "asc"      // asc/desc
);
}
