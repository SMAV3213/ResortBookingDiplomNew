using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResortBooking.Application.Interfaces.Services;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.API.Controllers;

/// <summary>
/// Контроллер управления типами комнат
/// </summary>
[ApiController]
[Route("api/room-types")]
public class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _service;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RoomTypesController"/>.
    /// </summary>
    /// <param name="service">Сервис бронирования.</param>
    public RoomTypesController(IRoomTypeService service)
    {
        _service = service;
    }

    /// <summary>
    /// Получить список всех типов комнат
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _service.GetAllAsync();
        return response.Success
            ? Ok(response.Data)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// Получить тип комнаты по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор типа комнаты</param>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _service.GetByIdAsync(id);
        return response.Success
            ? Ok(response.Data)
            : NotFound(response.Message);
    }

    /// <summary>
    /// Создать новый тип комнаты
    /// </summary>
    /// <remarks>
    /// Фотографии загружаются и сохраняются на сервере
    /// </remarks>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateRoomTypeDTO dto,
        [FromForm] List<IFormFile> images)
    {
        var response = await _service.CreateAsync(dto, images);
        if (!response.Success)
            return BadRequest(response.Message);

        return CreatedAtAction(nameof(GetById), new { id = response.Data }, response.Message);
    }

    /// <summary>
    /// Обновить тип комнаты
    /// </summary>
    /// <param name="id">Идентификатор типа комнаты</param>
    /// <param name="dto">Данные для обновления типа комнаты</param>
    /// <param name="images">Список новых фотографий (опционально)</param>
    /// <remarks>
    /// Старые фотографии будут удалены при замене
    /// </remarks>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromForm] UpdateRoomTypeDTO dto,
        [FromForm] List<IFormFile>? images)
    {
        var response = await _service.UpdateAsync(id, dto, images);
        return response.Success
            ? Ok(response.Message)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// Удалить тип комнаты
    /// </summary>
    /// <param name="id">Идентификатор типа комнаты</param>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _service.DeleteAsync(id);
        return response.Success
            ? Ok(response.Message)
            : NotFound(response.Message);
    }

    /// <summary>
    /// Получить свободные типы комнат на указанные даты
    /// </summary>
    /// <param name="guests">Количество гостей</param>
    /// <param name="checkIn">Дата заезда</param>
    /// <param name="checkOut">Дата выезда</param>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] int guests, [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
    {
        var response = await _service.GetAvailableRoomTypesAsync(guests, checkIn, checkOut);
        return response.Success
            ? Ok(response.Data)
            : BadRequest(response.Message);
    }
}
