namespace AcmHouse.Domain.Entities;

/// <summary>A tenant who rents a property through the agency.</summary>
public class Client
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    /// <summary>Belarusian passport number, e.g. MP1234567.</summary>
    public string PassportNumber { get; set; } = string.Empty;

    public List<Booking> Bookings { get; set; } = new();
}
