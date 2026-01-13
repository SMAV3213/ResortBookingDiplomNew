using ResortBooking.Application.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using static ResortBooking.Application.DTOs.BookingsDTOs;

namespace ResortBooking.Application.Interfaces.Services;

public interface IBookingService
{
    Task<ApiResponse<List<BookingDTO>>> GetAllAsync();
    Task<ApiResponse<BookingDTO>> GetByIdAsync(Guid id);
    Task<ApiResponse<Guid>> CreateAsync(CreateBookingDTO dto);
    Task<ApiResponse<bool>> UpdateAsync(Guid id, UpdateBookingDTO dto);
    Task<ApiResponse<bool>> CancelAsync(Guid id);
    Task<ApiResponse<List<BookingDTO>>> GetByUserIdAsync(Guid userId);
}
