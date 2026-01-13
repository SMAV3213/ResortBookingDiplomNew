using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entites;

public class Booking
{
    public Guid Id { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int GuestsCount { get; set; }

    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public BookingStatus Status { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
}
