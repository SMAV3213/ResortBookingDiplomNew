using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entites;

public class RoomTypeImage
{
    public Guid Id { get; set; }

    public string FilePath { get; set; } = null!;

    public Guid RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;
}
