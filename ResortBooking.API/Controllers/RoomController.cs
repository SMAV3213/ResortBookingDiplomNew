using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortBooking.Application.Interfaces.Services;
using static ResortBooking.Application.DTOs.RoomDTOs;

namespace ResortBooking.API.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

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
    public async Task<IActionResult> GetAll()
    {
        var response = await _roomService.GetAllAsync();
        return Ok(response);
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
        return response.Success ? Ok(response) : NotFound(response);
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
        return response.Success ? Ok(response) : BadRequest(response);
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
        return response.Success ? Ok(response) : BadRequest(response);
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
        return response.Success ? Ok(response) : NotFound(response);
    }
}
