namespace AcmHouse.Domain.Entities;

public static class Roles
{
    public const string Agent = "Agent";
    public const string Admin = "Admin";
}

/// <summary>An agency employee who signs in to the API: an agent or an administrator.</summary>
public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>PBKDF2 hash; the password itself is never stored.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary><see cref="Roles.Agent"/> or <see cref="Roles.Admin"/>.</summary>
    public string Role { get; set; } = Roles.Agent;

    public List<Booking> Bookings { get; set; } = new();
}
