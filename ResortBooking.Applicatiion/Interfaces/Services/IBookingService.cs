using ResortBooking.Application.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.BookingsDTOs;

namespace ResortBooking.Application.Interfaces.Services;

public interface IBookingService
{
    Task<ApiResponse<PagedResult<BookingDTO>>> GetAllAsync(BookingsQueryDTO query);
    Task<ApiResponse<PagedResult<BookingDTO>>> GetByUserIdAsync(Guid userId, BookingsQueryDTO query);

    Task<ApiResponse<BookingDTO>> GetByIdAsync(Guid id);
    Task<ApiResponse<Guid>> CreateAsync(CreateBookingDTO dto);
    Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateBookingDTO dto);
    Task<ApiResponse<bool>> CancelAsync(Guid id);
}
