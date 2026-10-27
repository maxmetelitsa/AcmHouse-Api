using AcmHouse.Application.Abstractions;
using AcmHouse.Application.Dtos;
using AcmHouse.Application.Services;
using AcmHouse.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcmHouse.Api.Controllers;

[Route("api/properties")]
public class PropertiesController(PropertyService properties) : ApiControllerBase
{
    /// <summary>Property catalog with filters and paging. Sorted by price, cheapest first.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PagedResult<PropertyDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PropertyDto>>> Search(
        [FromQuery] RentalType? rentalType,
        [FromQuery] PropertyType? type,
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? minRooms,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new PropertyQuery(rentalType, type, search, minPrice, maxPrice, minRooms, page, pageSize);
        return Ok(await properties.SearchAsync(query, ct));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<PropertyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyDto>> Get(int id, CancellationToken ct)
    {
        return Ok(await properties.GetAsync(id, ct));
    }

    /// <summary>Checks whether the property is free for the period and what the stay would cost.</summary>
    [HttpGet("{id:int}/availability")]
    [AllowAnonymous]
    [ProducesResponseType<AvailabilityDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailabilityDto>> Availability(int id, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        return Ok(await properties.CheckAvailabilityAsync(id, from, to, ct));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<PropertyDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PropertyDto>> Create(SavePropertyRequest request, CancellationToken ct)
    {
        var created = await properties.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<PropertyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyDto>> Update(int id, SavePropertyRequest request, CancellationToken ct)
    {
        return Ok(await properties.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await properties.DeleteAsync(id, ct);
        return NoContent();
    }
}
