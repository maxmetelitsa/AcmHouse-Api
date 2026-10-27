using AcmHouse.Application.Abstractions;
using AcmHouse.Domain.Entities;
using AcmHouse.Domain.Rules;

namespace AcmHouse.UnitTests.Fakes;

/// <summary>Simple in-memory storage behind the repository interfaces, for testing services without a database.</summary>
public class InMemoryStore : IUnitOfWork
{
    public List<Property> Properties { get; } = new();
    public List<Client> Clients { get; } = new();
    public List<Employee> Employees { get; } = new();
    public List<Booking> Bookings { get; } = new();

    public int SaveCalls { get; private set; }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCalls++;
        AssignIds(Properties);
        AssignIds(Clients);
        AssignIds(Employees);
        AssignIds(Bookings);

        foreach (var booking in Bookings)
        {
            booking.Property ??= Properties.FirstOrDefault(p => p.Id == booking.PropertyId);
            booking.Client ??= Clients.FirstOrDefault(c => c.Id == booking.ClientId);
            booking.Employee ??= Employees.FirstOrDefault(e => e.Id == booking.EmployeeId);
        }

        return Task.CompletedTask;
    }

    private static void AssignIds<T>(List<T> items) where T : class
    {
        var idProperty = typeof(T).GetProperty("Id")!;
        int next = items.Select(i => (int)idProperty.GetValue(i)!).DefaultIfEmpty(0).Max() + 1;
        foreach (var item in items.Where(i => (int)idProperty.GetValue(i)! == 0))
        {
            idProperty.SetValue(item, next++);
        }
    }
}

public class InMemoryPropertyRepository(InMemoryStore store) : IPropertyRepository
{
    public Task<PagedResult<Property>> SearchAsync(PropertyQuery query, CancellationToken ct = default)
    {
        IEnumerable<Property> items = store.Properties;

        if (query.RentalType is { } rentalType) items = items.Where(p => p.RentalType == rentalType);
        if (query.Type is { } type) items = items.Where(p => p.Type == type);
        if (query.Search is { } search)
            items = items.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                                     || p.Address.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (query.MinPrice is { } min) items = items.Where(p => p.DayPrice >= min);
        if (query.MaxPrice is { } max) items = items.Where(p => p.DayPrice <= max);
        if (query.MinRooms is { } rooms) items = items.Where(p => p.Rooms >= rooms);

        var filtered = items.OrderBy(p => p.DayPrice).ThenBy(p => p.Id).ToList();
        var page = filtered.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();
        return Task.FromResult(new PagedResult<Property>(page, query.Page, query.PageSize, filtered.Count));
    }

    public Task<Property?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(store.Properties.FirstOrDefault(p => p.Id == id));

    public Task AddAsync(Property property, CancellationToken ct = default)
    {
        store.Properties.Add(property);
        return Task.CompletedTask;
    }

    public void Remove(Property property) => store.Properties.Remove(property);
}

public class InMemoryClientRepository(InMemoryStore store) : IClientRepository
{
    public Task<IReadOnlyList<Client>> SearchAsync(string? search, CancellationToken ct = default)
    {
        IEnumerable<Client> items = store.Clients;
        if (search is not null)
        {
            items = items.Where(c => c.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
                                     || c.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
                                     || c.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                                     || c.PassportNumber.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<Client>>(items.OrderBy(c => c.LastName).ToList());
    }

    public Task<Client?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(store.Clients.FirstOrDefault(c => c.Id == id));

    public Task<bool> PassportExistsAsync(string passportNumber, CancellationToken ct = default) =>
        Task.FromResult(store.Clients.Any(c => c.PassportNumber == passportNumber));

    public Task AddAsync(Client client, CancellationToken ct = default)
    {
        store.Clients.Add(client);
        return Task.CompletedTask;
    }
}

public class InMemoryEmployeeRepository(InMemoryStore store) : IEmployeeRepository
{
    public Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        Task.FromResult(store.Employees.FirstOrDefault(e => string.Equals(e.Email, email, StringComparison.OrdinalIgnoreCase)));
}

public class InMemoryBookingRepository(InMemoryStore store) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(store.Bookings.FirstOrDefault(b => b.Id == id));

    public Task<IReadOnlyList<Booking>> GetOverlappingAsync(int propertyId, DateOnly start, DateOnly end, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Booking>>(store.Bookings
            .Where(b => b.PropertyId == propertyId && b.Status == BookingStatus.Active
                        && RentalRules.Overlaps(start, end, b.StartDate, b.EndDate))
            .OrderBy(b => b.StartDate)
            .ToList());

    public Task<IReadOnlyList<Booking>> ListAsync(int? propertyId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        IEnumerable<Booking> items = store.Bookings;
        if (propertyId is { } id) items = items.Where(b => b.PropertyId == id);
        if (from is { } f) items = items.Where(b => b.StartDate >= f);
        if (to is { } t) items = items.Where(b => b.StartDate <= t);
        return Task.FromResult<IReadOnlyList<Booking>>(items.OrderByDescending(b => b.StartDate).ThenBy(b => b.Id).ToList());
    }

    public Task<bool> AnyForPropertyAsync(int propertyId, CancellationToken ct = default) =>
        Task.FromResult(store.Bookings.Any(b => b.PropertyId == propertyId));

    public Task AddAsync(Booking booking, CancellationToken ct = default)
    {
        store.Bookings.Add(booking);
        return Task.CompletedTask;
    }
}

public class FixedClock(DateOnly today) : IClock
{
    public DateOnly Today { get; set; } = today;

    public DateTime UtcNow => Today.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
}

public class FakeTokenService : ITokenService
{
    public AccessToken CreateToken(Employee employee) =>
        new($"token-for-{employee.Id}", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));
}
