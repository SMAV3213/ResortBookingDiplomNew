using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortBooking.Application.Interfaces.Services;
using static ResortBooking.Application.DTOs.BookingsDTOs;

namespace ResortBooking.API.Controllers;


[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Получить все брони (только для администратора)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _bookingService.GetAllAsync();
        return Ok(response);
    }

    /// <summary>
    /// Получить конкретную бронь по Id
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _bookingService.GetByIdAsync(id);

        if (!User.IsInRole("Admin") && response.Success && response.Data?.UserId != Guid.Parse(User.FindFirst("sub")!.Value))
            return Forbid("Доступ запрещен к чужой брони");

        return response.Success ? Ok(response) : NotFound(response);
    }

    /// <summary>
    /// Получить брони текущего пользователя
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var response = await _bookingService.GetByUserIdAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Создать новую бронь
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDTO dto)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var dtoWithUser = dto with { UserId = userId };

        var response = await _bookingService.CreateAsync(dtoWithUser);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Обновить бронь (id в URL)
    /// </summary>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingDTO dto)
    {
        var response = await _bookingService.UpdateAsync(id, dto);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Отменить бронь
    /// </summary>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var response = await _bookingService.CancelAsync(id);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
