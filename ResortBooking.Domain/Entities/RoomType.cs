using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entites;

public class RoomType
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Capacity { get; set; }

    public decimal PricePerNight { get; set; }

    public ICollection<RoomTypeImage> Images { get; set; } = new List<RoomTypeImage>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
