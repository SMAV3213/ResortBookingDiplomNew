using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.DTOs;

public class RoomTypeDTOs
{
    // DTO для создания типа номера
    public class Create
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public List<string> ImagesUrls { get; set; } = new();
    }

    // DTO для обновления типа номера
    public class Update
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public List<string> ImagesUrls { get; set; } = new();
    }

    // DTO для ответа фронта
    public class Response
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public List<string> ImagesUrls { get; set; } = new();
    }
}
