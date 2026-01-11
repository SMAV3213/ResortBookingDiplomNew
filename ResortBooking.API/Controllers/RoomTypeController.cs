using Microsoft.AspNetCore.Mvc;
using ResortBooking.Application.DTOs;
using ResortBooking.Application.Services;
using ResortBooking.Domain.Entities;
using ResortBooking.Infrastructure.Persistence;

namespace ResortBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomTypeController : ControllerBase
{
    private readonly RoomTypeService _service;

    public RoomTypeController(RoomTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var rt = await _service.GetByIdAsync(id);
        return rt == null ? NotFound("Тип номера не найден") : Ok(rt);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoomTypeDTOs.Create request)
    {
        await _service.CreateAsync(request);
        return Ok("Тип номера успешно создан");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoomTypeDTOs.Update request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result ? Ok("Тип номера успешно обновлён") : NotFound("Тип номера не найден");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? Ok("Тип номера успешно удалён") : NotFound("Тип номера не найден");
    }
}