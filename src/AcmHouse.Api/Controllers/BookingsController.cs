using AcmHouse.Application.Dtos;
using AcmHouse.Application.Services;
using AcmHouse.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcmHouse.Api.Controllers;

[Route("api/bookings")]
[Authorize]
public class BookingsController(BookingService bookings) : ApiControllerBase
{
    /// <summary>Signs a rental contract. The price is calculated by the server; overlapping dates return 409.</summary>
    [HttpPost]
    [ProducesResponseType<BookingDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request, CancellationToken ct)
    {
        var created = await bookings.CreateAsync(request, CurrentEmployeeId, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Bookings, optionally for one property and with a start date in [from, to].</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BookingDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> List(
        [FromQuery] int? propertyId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        return Ok(await bookings.ListAsync(propertyId, from, to, ct));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<BookingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> Get(int id, CancellationToken ct)
    {
        return Ok(await bookings.GetAsync(id, ct));
    }

    /// <summary>Cancels a booking that has not started yet.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        await bookings.CancelAsync(id, CurrentEmployeeId, User.IsInRole(Roles.Admin), ct);
        return NoContent();
    }
}
