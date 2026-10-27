using AcmHouse.Domain.Entities;
using AcmHouse.Infrastructure.Security;

namespace AcmHouse.UnitTests.Fakes;

/// <summary>A small agency: an admin, two agents, three properties and two clients. Today is 1 May 2026.</summary>
public class TestData
{
    public static readonly DateOnly Today = new(2026, 5, 1);

    public InMemoryStore Store { get; } = new();
    public FixedClock Clock { get; } = new(Today);
    public Pbkdf2PasswordHasher Hasher { get; } = new();

    public Employee Admin { get; }
    public Employee Agent { get; }
    public Employee OtherAgent { get; }
    public Property Flat { get; }
    public Property House { get; }
    public Property Office { get; }
    public Client Client { get; }

    public TestData()
    {
        Admin = new Employee { Id = 1, FirstName = "Admin", LastName = "Manager", Email = "admin@acmhouse.by", PasswordHash = Hasher.Hash("Admin1234"), Role = Roles.Admin };
        Agent = new Employee { Id = 2, FirstName = "Иван", LastName = "Петров", Email = "ivan.petrov@acmhouse.by", PasswordHash = Hasher.Hash("Demo1234"), Role = Roles.Agent };
        OtherAgent = new Employee { Id = 3, FirstName = "Анна", LastName = "Ковалёва", Email = "anna.kovaleva@acmhouse.by", PasswordHash = Hasher.Hash("Demo1234"), Role = Roles.Agent };
        Store.Employees.AddRange(new[] { Admin, Agent, OtherAgent });

        Flat = new Property { Id = 1, Name = "Квартира в центре", Type = PropertyType.Apartment, RentalType = RentalType.Residential, Address = "пр-т Независимости 10", Rooms = 2, Area = 54.5m, DayPrice = 120m };
        House = new Property { Id = 2, Name = "Стильный дом", Type = PropertyType.House, RentalType = RentalType.Residential, Address = "ул. Революционная 67", Rooms = 4, Area = 140m, DayPrice = 250m };
        Office = new Property { Id = 3, Name = "Офис на Немиге", Type = PropertyType.Office, RentalType = RentalType.Commercial, Address = "ул. Козлова 82", Rooms = 10, Area = 240m, DayPrice = 180m };
        Store.Properties.AddRange(new[] { Flat, House, Office });

        Client = new Client { Id = 1, FirstName = "Олег", LastName = "Сидоренко", Email = "sidorenko@example.com", Phone = "+375 29 000-00-01", PassportNumber = "MP4567890" };
        Store.Clients.Add(Client);
        Store.Clients.Add(new Client { Id = 2, FirstName = "Дмитрий", LastName = "Ермолаев", Email = "ermolaev@example.com", Phone = "+375 29 000-00-02", PassportNumber = "MP5678901" });
    }

    public InMemoryPropertyRepository Properties => new(Store);
    public InMemoryClientRepository Clients => new(Store);
    public InMemoryEmployeeRepository Employees => new(Store);
    public InMemoryBookingRepository Bookings => new(Store);

    /// <summary>Adds an existing booking of the flat, signed by <see cref="Agent"/>.</summary>
    public Booking AddBooking(DateOnly start, DateOnly end, BookingStatus status = BookingStatus.Active, Employee? employee = null, Property? property = null)
    {
        var p = property ?? Flat;
        var e = employee ?? Agent;
        var booking = new Booking
        {
            Id = Store.Bookings.Count + 1,
            PropertyId = p.Id, Property = p,
            ClientId = Client.Id, Client = Client,
            EmployeeId = e.Id, Employee = e,
            StartDate = start, EndDate = end,
            TotalPrice = (end.DayNumber - start.DayNumber) * p.DayPrice,
            AgentProfit = Domain.Rules.RentalRules.CalculateAgentProfit((end.DayNumber - start.DayNumber) * p.DayPrice),
            Status = status
        };
        Store.Bookings.Add(booking);
        return booking;
    }
}
