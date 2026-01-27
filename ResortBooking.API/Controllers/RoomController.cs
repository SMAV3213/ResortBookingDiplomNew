using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortBooking.Application.Interfaces.Services;
using static ResortBooking.Application.DTOs.RoomDTOs;

namespace ResortBooking.API.Controllers;

/// <summary>
/// Контроллер управления комнатами
/// </summary>
[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RoomController"/>.
    /// </summary>
    /// <param name="roomService">Сервис бронирования.</param>
    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Получить список всех комнат
    /// </summary>
    /// <remarks>Доступно только администратору</remarks>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] RoomsQueryDTO query)
    {
        var response = await _roomService.GetAllAsync(query);
        return response.Success ? Ok(response.Data) : BadRequest(response.Message);
    }

    /// <summary>
    /// Получить комнату по Id
    /// </summary>
    /// <remarks>Доступно только администратору</remarks>
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _roomService.GetByIdAsync(id);
        return response.Success
            ? Ok(response.Data)
            : NotFound(response.Message);
    }

    /// <summary>
    /// Создать новую комнату
    /// </summary>
    /// <remarks>Доступно только администратору</remarks>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomDTO dto)
    {
        var response = await _roomService.AddAsync(dto);
        if (!response.Success)
            return BadRequest(response.Message);

        return CreatedAtAction(nameof(GetById), new { id = response.Data }, response.Message);
    }

    /// <summary>
    /// Обновить данные комнаты
    /// </summary>
    /// <remarks>Доступно только администратору</remarks>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomDTO dto)
    {
        var response = await _roomService.UpdateAsync(id, dto);
        return response.Success
            ? Ok(response.Message)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// Удалить комнату
    /// </summary>
    /// <remarks>Доступно только администратору</remarks>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _roomService.DeleteAsync(id);
        return response.Success
            ? Ok(response.Message)
            : NotFound(response.Message);
    }
}
