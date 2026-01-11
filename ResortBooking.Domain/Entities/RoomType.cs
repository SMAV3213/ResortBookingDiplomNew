using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entities;

public class RoomType : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public ICollection<RoomTypeImage> Images { get; set; } = new List<RoomTypeImage>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
