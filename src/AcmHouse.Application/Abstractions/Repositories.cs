using AcmHouse.Domain.Entities;

namespace AcmHouse.Application.Abstractions;

/// <summary>Filters for the property catalog. Every filter is optional.</summary>
public record PropertyQuery(
    RentalType? RentalType = null,
    PropertyType? Type = null,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int? MinRooms = null,
    int Page = 1,
    int PageSize = 20);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public interface IPropertyRepository
{
    Task<PagedResult<Property>> SearchAsync(PropertyQuery query, CancellationToken ct = default);

    Task<Property?> GetByIdAsync(int id, CancellationToken ct = default);

    Task AddAsync(Property property, CancellationToken ct = default);

    void Remove(Property property);
}

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> SearchAsync(string? search, CancellationToken ct = default);

    Task<Client?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> PassportExistsAsync(string passportNumber, CancellationToken ct = default);

    Task AddAsync(Client client, CancellationToken ct = default);
}

public interface IEmployeeRepository
{
    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default);
}

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Active bookings of the property that overlap the period [start, end).</summary>
    Task<IReadOnlyList<Booking>> GetOverlappingAsync(int propertyId, DateOnly start, DateOnly end, CancellationToken ct = default);

    /// <summary>Bookings with their property, client and employee, filtered by property and start date.</summary>
    Task<IReadOnlyList<Booking>> ListAsync(int? propertyId, DateOnly? from, DateOnly? to, CancellationToken ct = default);

    Task<bool> AnyForPropertyAsync(int propertyId, CancellationToken ct = default);

    Task AddAsync(Booking booking, CancellationToken ct = default);
}

/// <summary>Saves all changes made through the repositories in one transaction.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
