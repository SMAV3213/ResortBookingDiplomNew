using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entities;

public class Booking : BaseEntity
{
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    public int GuestsCount { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
}