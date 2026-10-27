namespace AcmHouse.Domain.Entities;

public enum PropertyType
{
    Apartment = 1,
    House = 2,
    Villa = 3,
    Cottage = 4,
    Office = 5,
    Warehouse = 6
}

public enum RentalType
{
    Residential = 0,
    Commercial = 1
}

/// <summary>A property the agency rents out.</summary>
public class Property
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public PropertyType Type { get; set; }

    public RentalType RentalType { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? FlatNumber { get; set; }

    public int Rooms { get; set; }

    /// <summary>Area in square metres.</summary>
    public decimal Area { get; set; }

    /// <summary>Price for one day of rent, BYN.</summary>
    public decimal DayPrice { get; set; }

    public List<Booking> Bookings { get; set; } = new();
}
