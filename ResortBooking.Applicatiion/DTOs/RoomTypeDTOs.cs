using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.DTOs;

public class RoomTypeDTOs
{
    public record RoomTypeDTO
    (
        Guid Id,
        string Name,
        string Description,
        int Capacity,
        decimal PricePerNight,
        List<string> ImageUrls,
        List<RoomsInRoomTypeDTO> Rooms
    );
    public record RoomTypeWithoutRoomsDTO
    (
        Guid Id,
        string Name,
        string Description,
        int Capacity,
        decimal PricePerNight,
        List<string> ImageUrls
    );

    public record CreateRoomTypeDTO
    (
        string Name,
        string Description,
        int Capacity,
        decimal PricePerNight
    );
    public record UpdateRoomTypeDTO
    (
        string Name,
        string Description,
        int Capacity,
        decimal PricePerNight
    );

    public record RoomsInRoomTypeDTO
    (
        Guid Id,
        string Number,
        string Status
    );
}
