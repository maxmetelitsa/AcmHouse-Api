using AcmHouse.Application.Abstractions;
using AcmHouse.Application.Dtos;
using AcmHouse.Application.Exceptions;
using AcmHouse.Domain.Entities;

namespace AcmHouse.Application.Services;

public class ReportService(IBookingRepository bookings)
{
    /// <summary>Revenue from active bookings that start within [from, to], in total and per employee.</summary>
    public async Task<RevenueReportDto> GetRevenueAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (to < from)
        {
            throw new BusinessRuleException("'to' cannot be earlier than 'from'.");
        }

        var active = (await bookings.ListAsync(null, from, to, ct))
            .Where(b => b.Status == BookingStatus.Active)
            .ToList();

        var byEmployee = active
            .GroupBy(b => b.EmployeeId)
            .Select(g => new EmployeeRevenueDto(
                g.Key,
                g.First().Employee is { } e ? $"{e.FirstName} {e.LastName}" : string.Empty,
                g.Count(),
                g.Sum(b => b.TotalPrice),
                g.Sum(b => b.AgentProfit)))
            .OrderByDescending(r => r.Revenue)
            .ToList();

        return new RevenueReportDto(from, to, active.Count, active.Sum(b => b.TotalPrice), byEmployee);
    }
}
