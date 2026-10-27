using AcmHouse.Application.Dtos;
using AcmHouse.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcmHouse.Api.Controllers;

[Route("api/auth")]
public class AuthController(AuthService auth) : ApiControllerBase
{
    /// <summary>Signs an employee in and returns a JWT access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        return Ok(await auth.LoginAsync(request, ct));
    }
}
