using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Enums
{
    public enum RoomStatus
    {
        Available = 0,        // Свободен
        Booked = 1,           // Забронирован
        Occupied = 2,         // Заселён
        Cleaning = 3,         // Уборка
        Maintenance = 4,      // Ремонт / недоступен
        OutOfService = 5      // Выведен из эксплуатации
    }
}
