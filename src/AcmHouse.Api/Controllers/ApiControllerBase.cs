using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AcmHouse.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>ID of the signed-in employee, taken from the access token.</summary>
    protected int CurrentEmployeeId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id)
            ? id
            : throw new UnauthorizedAccessException("The access token has no employee ID.");
}
