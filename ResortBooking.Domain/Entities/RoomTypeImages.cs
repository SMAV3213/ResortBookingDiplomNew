using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entities;

public class RoomTypeImage : BaseEntity
{
    public string ImageUrl { get; set; } = null!;
    public bool IsMain { get; set; }

    public Guid RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;
}
