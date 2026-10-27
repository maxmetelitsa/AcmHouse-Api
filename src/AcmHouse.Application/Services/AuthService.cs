using AcmHouse.Application.Abstractions;
using AcmHouse.Application.Dtos;
using AcmHouse.Application.Exceptions;

namespace AcmHouse.Application.Services;

public class AuthService(IEmployeeRepository employees, IPasswordHasher hasher, ITokenService tokens)
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var employee = await employees.GetByEmailAsync(request.Email.Trim(), ct);

        // The same error for an unknown email and a wrong password, so the API does not reveal which emails exist.
        if (employee is null || !hasher.Verify(request.Password, employee.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        AccessToken token = tokens.CreateToken(employee);
        return new LoginResponse(token.Token, token.ExpiresAtUtc, employee.Role, $"{employee.FirstName} {employee.LastName}");
    }
}
