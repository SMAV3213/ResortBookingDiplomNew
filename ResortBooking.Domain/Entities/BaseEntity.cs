using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
