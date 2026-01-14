using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ResortBooking.Application.DTOs;
using ResortBooking.Application.Interfaces.Repositories;
using ResortBooking.Application.Interfaces.Services;
using ResortBooking.Application.Responses;
using ResortBooking.Domain.Entites;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.Infrastructure.Services;

public class RoomTypeService : IRoomTypeService
{
    private readonly IRoomTypeRepository _repository;
    private readonly IWebHostEnvironment _env;

    public RoomTypeService(IRoomTypeRepository repository, IWebHostEnvironment env)
    {
        _repository = repository;
        _env = env;
    }

    public async Task<ApiResponse<List<RoomTypeDTO>>> GetAllAsync()
    {
        var types = await _repository.GetAllAsync();

        var result = types.Select(x => new RoomTypeDTO(
            x.Id,
            x.Name,
            x.Description,
            x.Capacity,
            x.PricePerNight,
            x.Images.Select(i => i.FilePath).ToList(),
            x.Rooms.Select(r => new RoomsInRoomTypeDTO(
                r.Id,
                r.Number,
                r.Status.ToString()
            )).ToList()
        )).ToList();

        return ApiResponse<List<RoomTypeDTO>>.Ok(result, "Типы комнат успешно получены");
    }

    public async Task<ApiResponse<RoomTypeDTO>> GetByIdAsync(Guid id)
    {
        var type = await _repository.GetByIdAsync(id);
        if (type == null)
            return ApiResponse<RoomTypeDTO>.Fail("Тип комнаты не найден");

        var dto = new RoomTypeDTO(
            type.Id,
            type.Name,
            type.Description,
            type.Capacity,
            type.PricePerNight,
            type.Images.Select(i => i.FilePath).ToList(),
            type.Rooms.Select(r => new RoomsInRoomTypeDTO(
                r.Id,
                r.Number,
                r.Status.ToString()
            )).ToList()
        );

        return ApiResponse<RoomTypeDTO>.Ok(dto, "Тип комнаты получен");
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreateRoomTypeDTO dto, List<IFormFile> images)
    {
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Capacity = dto.Capacity,
            PricePerNight = dto.PricePerNight
        };

        await SaveImagesAsync(roomType, images);

        var roomTypeDto = new RoomTypeWithoutRoomsDTO(
            roomType.Id,
            roomType.Name,
            roomType.Description,
            roomType.Capacity,
            roomType.PricePerNight,
            roomType.Images.Select(i => i.FilePath).ToList()
        );

        await _repository.AddAsync(roomTypeDto);
        await _repository.SaveChangesAsync();

        return ApiResponse<Guid>.Ok(roomType.Id, "Тип комнаты успешно создан");
    }

    public async Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateRoomTypeDTO dto, List<IFormFile>? images)
    {
        var roomType = await _repository.GetByIdAsync(id);
        if (roomType == null)
            return ApiResponse<bool>.Fail("Тип комнаты не найден");

        roomType.Name = dto.Name;
        roomType.Description = dto.Description;
        roomType.Capacity = dto.Capacity;
        roomType.PricePerNight = dto.PricePerNight;

        if (images != null && images.Any())
        {
            DeleteImages(roomType);
            roomType.Images.Clear();
            await SaveImagesAsync(roomType, images);
        }

        _repository.Update(roomType);
        await _repository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Тип комнаты обновлён");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var roomType = await _repository.GetByIdAsync(id);
        if (roomType == null)
            return ApiResponse<bool>.Fail("Тип комнаты не найден");

        DeleteImages(roomType);

        _repository.Remove(roomType);
        await _repository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Тип комнаты удалён");
    }


    private async Task SaveImagesAsync(RoomType roomType, List<IFormFile> images)
    {
        var webRoot = _env.WebRootPath 
                      ?? Path.Combine(_env.ContentRootPath ?? string.Empty, "wwwroot");

        var folder = Path.Combine(webRoot, "uploads", "room-types");
        Directory.CreateDirectory(folder);

        foreach (var image in images)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await image.CopyToAsync(stream);

            roomType.Images.Add(new RoomTypeImage
            {
                Id = Guid.NewGuid(),
                FilePath = $"/uploads/room-types/{fileName}"
            });
        }
    }

    private void DeleteImages(RoomType roomType)
    {
        foreach (var image in roomType.Images)
        {
            var fullPath = Path.Combine(_env.WebRootPath, image.FilePath.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }

    public async Task<ApiResponse<List<RoomTypeWithoutRoomsDTO>>> GetAvailableRoomTypesAsync(int guests, DateTime checkIn, DateTime checkOut)
    {
        var types = await _repository.GetAvailableRoomTypesAsync(guests, checkIn, checkOut);
        return ApiResponse<List<RoomTypeWithoutRoomsDTO>>.Ok(types, "Свободные типы комнат получены");
    }

}
