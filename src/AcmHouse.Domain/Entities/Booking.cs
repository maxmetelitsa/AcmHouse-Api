namespace AcmHouse.Domain.Entities;

public enum BookingStatus
{
    Active = 0,
    Cancelled = 1
}

/// <summary>A rental contract: a property rented to a client for a period, signed by an agent.</summary>
public class Booking
{
    public int Id { get; set; }

    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public int ClientId { get; set; }

    public Client? Client { get; set; }

    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    /// <summary>First day of the rent.</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>Day the tenant leaves; the property is free again from this day.</summary>
    public DateOnly EndDate { get; set; }

    public decimal TotalPrice { get; set; }

    /// <summary>Part of the price credited to the agent (commission minus tax).</summary>
    public decimal AgentProfit { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Active;

    public DateTime CreatedAtUtc { get; set; }
}
