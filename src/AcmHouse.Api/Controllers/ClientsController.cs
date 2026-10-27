using AcmHouse.Application.Dtos;
using AcmHouse.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcmHouse.Api.Controllers;

[Route("api/clients")]
[Authorize]
public class ClientsController(ClientService clients) : ApiControllerBase
{
    /// <summary>Clients, optionally filtered by name, email or passport number.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClientDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientDto>>> Search([FromQuery] string? search, CancellationToken ct)
    {
        return Ok(await clients.SearchAsync(search, ct));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ClientDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> Get(int id, CancellationToken ct)
    {
        return Ok(await clients.GetAsync(id, ct));
    }

    [HttpPost]
    [ProducesResponseType<ClientDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientDto>> Create(CreateClientRequest request, CancellationToken ct)
    {
        var created = await clients.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
