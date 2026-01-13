using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entites;

public class Room
{
    public Guid Id { get; set; }

    public string Number { get; set; } = null!;

    public RoomStatus Status { get; set; }

    public Guid RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
