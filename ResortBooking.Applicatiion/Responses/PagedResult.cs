using System;
using System.Collections.Generic;
using System.Text;

namespace ResortBooking.Application.Responses;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Total { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}
