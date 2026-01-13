using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortBooking.Application.Interfaces.Services;
using static ResortBooking.Application.DTOs.RoomTypeDTOs;

namespace ResortBooking.API.Controllers;

[ApiController]
[Route("api/room-types")]
public class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _service;

    public RoomTypesController(IRoomTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _service.GetByIdAsync(id));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoomTypeDTO dto,
        [FromForm] List<IFormFile> images)
        => Ok(await _service.CreateAsync(dto, images));

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRoomTypeDTO dto,
        [FromForm] List<IFormFile>? images)
        => Ok(await _service.UpdateAsync(id, dto, images));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => Ok(await _service.DeleteAsync(id));

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] int guests , [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
    {
        return Ok(await _service.GetAvailableRoomTypesAsync(guests ,checkIn, checkOut));
    }
}
