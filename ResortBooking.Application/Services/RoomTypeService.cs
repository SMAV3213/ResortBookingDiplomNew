using ResortBooking.Application.DTOs;
using ResortBooking.Application.Interfaces;
using ResortBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.Services;

public class RoomTypeService
{
    private readonly IRoomTypeRepository _repo;

    public RoomTypeService(IRoomTypeRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<RoomTypeDTOs.Response>> GetAllAsync()
    {
        var roomTypes = await _repo.GetAllAsync();
        return roomTypes.Select(rt => new RoomTypeDTOs.Response
        {
            Id = rt.Id,
            Name = rt.Name,
            Description = rt.Description,
            PricePerNight = rt.PricePerNight,
            MaxGuests = rt.MaxGuests,
            ImagesUrls = rt.Images.Select(i => i.ImageUrl).ToList()
        }).ToList();
    }

    public async Task<RoomTypeDTOs.Response?> GetByIdAsync(Guid id)
    {
        var rt = await _repo.GetByIdAsync(id);
        if (rt == null) return null;

        return new RoomTypeDTOs.Response
        {
            Id = rt.Id,
            Name = rt.Name,
            Description = rt.Description,
            PricePerNight = rt.PricePerNight,
            MaxGuests = rt.MaxGuests,
            ImagesUrls = rt.Images.Select(i => i.ImageUrl).ToList()
        };
    }

    public async Task CreateAsync(RoomTypeDTOs.Create request)
    {
        var roomType = new RoomType
        {
            Name = request.Name,
            Description = request.Description,
            PricePerNight = request.PricePerNight,
            MaxGuests = request.MaxGuests,
            Images = request.ImagesUrls.Select(url => new RoomTypeImage { ImageUrl = url }).ToList()
        };

        await _repo.AddAsync(roomType);
    }

    public async Task<bool> UpdateAsync(Guid id, RoomTypeDTOs.Update request)
    {
        var roomType = await _repo.GetByIdAsync(id);
        if (roomType == null) return false;

        roomType.Name = request.Name;
        roomType.Description = request.Description;
        roomType.PricePerNight = request.PricePerNight;
        roomType.MaxGuests = request.MaxGuests;

        roomType.Images.Clear();
        roomType.Images = request.ImagesUrls.Select(url => new RoomTypeImage { ImageUrl = url }).ToList();

        await _repo.UpdateAsync(roomType);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var roomType = await _repo.GetByIdAsync(id);
        if (roomType == null) return false;

        await _repo.DeleteAsync(roomType);
        return true;
    }
}