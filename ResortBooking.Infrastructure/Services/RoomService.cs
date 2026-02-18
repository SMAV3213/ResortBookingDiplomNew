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

/// <summary>
/// Сервис для управления номерами в отеле
/// 
/// Отвечает за:
/// - CRUD операции с номерами (Create, Read, Update, Delete)
/// - Поиск и фильтрацию номеров
/// - Получение доступных номеров на конкретные даты
/// - Пагинацию и сортировку
/// </summary>
public class RoomService : IRoomService
{
    private readonly IRoomRepository _repository;

    public RoomService(IRoomRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Получить все номера с фильтром, поиском и пагинацией
    /// 
    /// Параметры query могут содержать:
    /// - Page: номер страницы (по умолчанию 1)
    /// - PageSize: количество элементов на странице
    /// - Search: поиск по номеру комнаты (101, 102...)
    /// - Status: фильтр по статусу (Available, Occupied, Maintenance)
    /// - RoomTypeId: фильтр по типу номера
    /// - SortBy: поле для сортировки (number, createdAt...)
    /// - SortDir: направление сортировки (asc, desc)
    /// </summary>
    public async Task<ApiResponse<PagedResult<RoomDTO>>> GetAllAsync(RoomsQueryDTO query)
    {
        // Получаем страничные данные из репозитория
        // SearchAsync выполняет поиск, фильтрацию и пагинацию в одном запросе к БД
        var paged = await _repository.SearchAsync(query);

        // Преобразуем доменные модели (Room) в DTO для отправки клиенту
        // Это нужно чтобы не отправлять лишние внутренние данные
        var items = paged.Items.Select(x => new RoomDTO(
            x.Id,
            x.Number,
            x.Status.ToString(),
            // Включаем информацию о типе номера
            new RoomTypeInRoomsDTO(
                x.RoomType.Id,
                x.RoomType.Name,
                x.RoomType.Description,
                x.RoomType.Capacity,
                x.RoomType.PricePerNight
            )
        )).ToList();

        // Возвращаем результат с информацией о пагинации
        return ApiResponse<PagedResult<RoomDTO>>.Ok(new PagedResult<RoomDTO>
        {
            Items = items,
            Total = paged.Total,           // Общее количество элементов
            Page = paged.Page,             // Текущая страница
            PageSize = paged.PageSize      // Размер страницы
        }, "Комнаты успешно получены");
    }

    /// <summary>
    /// Получить конкретный номер по ID
    /// </summary>
    public async Task<ApiResponse<RoomDTO>> GetByIdAsync(Guid id)
    {
        // Ищем номер в БД с загрузкой связанного типа
        var room = await _repository.GetByIdAsync(id);
        if (room == null)
            return ApiResponse<RoomDTO>.Fail("Комната не найдена");

        // Преобразуем в DTO
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

    /// <summary>
    /// Создать новый номер в отеле
    /// 
    /// Процесс:
    /// 1. Создаём объект Room с новым ID
    /// 2. Новый номер всегда имеет статус Available (свободен)
    /// 3. Сохраняем в БД
    /// 4. Возвращаем ID созданного номера
    /// </summary>
    public async Task<ApiResponse<Guid>> AddAsync(CreateRoomDTO dto)
    {
        // Создаём объект номера
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Number = dto.Number,
            RoomTypeId = dto.RoomTypeId,
            Status = RoomStatus.Available,  // Новый номер всегда свободен
            CreatedAt = DateTime.UtcNow
        };

        // Сохраняем в БД
        await _repository.AddAsync(room);
        await _repository.SaveChangesAsync();

        return ApiResponse<Guid>.Ok(
            room.Id,
            "Комната успешно создана");
    }

    /// <summary>
    /// Обновить данные номера (номер, тип, статус)
    /// 
    /// Процесс:
    /// 1. Ищем номер в БД
    /// 2. Обновляем его поля
    /// 3. Проверяем что статус корректный
    /// 4. Сохраняем изменения
    /// </summary>
    public async Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateRoomDTO dto)
    {
        // Ищем номер в БД
        var room = await _repository.GetByIdAsync(id);
        if (room == null)
            return ApiResponse<bool>.Fail("Комната не найдена");

        // Обновляем номер и тип
        room.Number = dto.Number;
        room.RoomTypeId = dto.RoomTypeId;

        // Проверяем что статус - это допустимое значение enum
        if (!Enum.TryParse<RoomStatus>(dto.Status, out var status))
            return ApiResponse<bool>.Fail("Некорректный статус комнаты");

        room.Status = status;

        // Сохраняем в БД
        _repository.Update(room);
        await _repository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Комната успешно обновлена");
    }

    /// <summary>
    /// Удалить номер из системы
    /// 
    /// ВНИМАНИЕ: Нужно быть осторожным при удалении!
    /// Если на номер есть активные бронирования - может быть ошибка в БД
    /// (или нужно удалить сначала бронирования, или использовать soft delete)
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        // Ищем номер в БД
        var room = await _repository.GetByIdAsync(id);
        if (room == null)
            return ApiResponse<bool>.Fail("Комната не найдена");

        // Удаляем
        _repository.Delete(room);
        await _repository.SaveChangesAsync();

        return ApiResponse<bool>.Ok(
            true,
            "Комната успешно удалена");
    }

    /// <summary>
    /// Получить доступные номера определённого типа на конкретные даты
    /// 
    /// Используется при создании бронирования - нужно найти
    /// свободный номер нужного типа на эти даты
    /// 
    /// Параметры:
    /// - roomTypeId: какой тип номера ищем (Люкс, Стандарт и т.д.)
    /// - checkIn: дата заезда
    /// - checkOut: дата выезда
    /// </summary>
    public async Task<ApiResponse<List<RoomDTO>>> GetAvailableRoomsAsync(
        Guid roomTypeId, 
        DateTime checkIn, 
        DateTime checkOut)
    {
        // Получаем все номера этого типа, которые не забронированы на эти даты
        var rooms = await _repository.GetAvailableByRoomTypeAsync(roomTypeId);

        // Преобразуем в DTO
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

