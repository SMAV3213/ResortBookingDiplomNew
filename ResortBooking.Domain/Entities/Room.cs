using ResortBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entities;

public class Room : BaseEntity
{
    public string Name { get; set; } = null!;
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    public Guid RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}