using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Enums;

public enum BookingStatus
{
    Created = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}
