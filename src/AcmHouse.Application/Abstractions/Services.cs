using AcmHouse.Domain.Entities;

namespace AcmHouse.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}

public record AccessToken(string Token, DateTime ExpiresAtUtc);

public interface ITokenService
{
    AccessToken CreateToken(Employee employee);
}

/// <summary>Current time, replaceable in tests.</summary>
public interface IClock
{
    DateTime UtcNow { get; }

    DateOnly Today { get; }
}

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
