using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortBooking.Application.Interfaces.Services;
using static ResortBooking.Application.DTOs.UserDTOs;

namespace ResortBooking.API.Controllers;

/// <summary>
/// Контроллер управления пользователями
/// </summary>
[ApiController]
[Route("api/users")]

public class UserController : ControllerBase
{
    private readonly IUserService _service;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="UserController"/>.
    /// </summary>
    /// <param name="service">Сервис бронирования.</param>
    public UserController(IUserService service)
    {
        _service = service;
    }

    /// <summary>
    /// Получить список всех пользователей.
    /// Доступ к методу разрешён только пользователям с ролью "Admin".
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _service.GetAllAsync();
        return response.Success
            ? Ok(response.Data)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// Получить пользователя по идентификатору.
    /// Требуется аутентификация (любой авторизованный пользователь).
    /// </summary>
    /// <param name="id">Идентификатор пользователя (GUID).</param>
    [HttpGet("{id:guid}")]
    [Authorize]

    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _service.GetByIdAsync(id);
        return response.Success
            ? Ok(response.Data)
            : NotFound(response.Message);
    }

    /// <summary>
    /// Обновить данные пользователя.
    /// Требуется аутентификация (пользователь может обновлять собственные данные или администратор).
    /// </summary>
    /// <param name="id">Идентификатор пользователя (GUID) для обновления.</param>
    /// <param name="dto">DTO с полями для обновления пользователя.</param>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateUserDTO dto)
    {
        var response = await _service.UpdateAsync(id, dto);
        return response.Success
            ? Ok(response.Message)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// Изменить роль пользователя.
    /// Доступно только администраторам.
    /// </summary>
    /// <param name="id">Идентификатор пользователя (GUID), роль которого нужно изменить.</param>
    /// <param name="dto">DTO с новой ролью пользователя.</param>
    [HttpPatch("{id:guid}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeRole(Guid id, ChangeUserRoleDTO dto)
    {
        var response = await _service.ChangeRoleAsync(id, dto);
        return response.Success
            ? Ok(response.Message)
            : BadRequest(response.Message);
    }

    /// <summary>
    /// Удалить пользователя по идентификатору.
    /// Доступно только администраторам.
    /// </summary>
    /// <param name="id">Идентификатор пользователя (GUID) для удаления.</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _service.DeleteAsync(id);
        return response.Success
            ? Ok(response.Message)
            : NotFound(response.Message);
    }
}
