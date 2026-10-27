using AcmHouse.Application.Dtos;
using AcmHouse.Application.Services;
using AcmHouse.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcmHouse.Api.Controllers;

[Route("api/reports")]
[Authorize(Roles = Roles.Admin)]
public class ReportsController(ReportService reports) : ApiControllerBase
{
    /// <summary>Revenue and agent profit for bookings starting within the period.</summary>
    [HttpGet("revenue")]
    [ProducesResponseType<RevenueReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RevenueReportDto>> Revenue([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        return Ok(await reports.GetRevenueAsync(from, to, ct));
    }
}
