using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AcmHouse.Application.Abstractions;
using AcmHouse.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AcmHouse.Api.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "AcmHouse.Api";

    public string Audience { get; set; } = "AcmHouse.Clients";

    /// <summary>HMAC-SHA256 signing key, at least 32 characters. Set it through configuration, never in code.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpiresMinutes { get; set; } = 60;

    public SymmetricSecurityKey GetSigningKey() => new(Encoding.UTF8.GetBytes(Key));
}

public class JwtTokenService(IOptions<JwtOptions> options, IClock clock) : ITokenService
{
    private readonly JwtOptions jwt = options.Value;

    public AccessToken CreateToken(Employee employee)
    {
        DateTime expires = clock.UtcNow.AddMinutes(jwt.ExpiresMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Email, employee.Email),
            new Claim(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}"),
            new Claim(ClaimTypes.Role, employee.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            notBefore: clock.UtcNow,
            expires: expires,
            signingCredentials: new SigningCredentials(jwt.GetSigningKey(), SecurityAlgorithms.HmacSha256));

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
