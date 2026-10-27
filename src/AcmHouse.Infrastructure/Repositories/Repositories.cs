using AcmHouse.Application.Abstractions;
using AcmHouse.Domain.Entities;
using AcmHouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcmHouse.Infrastructure.Repositories;

public class PropertyRepository(AppDbContext db) : IPropertyRepository
{
    public async Task<PagedResult<Property>> SearchAsync(PropertyQuery query, CancellationToken ct = default)
    {
        IQueryable<Property> properties = db.Properties.AsNoTracking();

        if (query.RentalType is { } rentalType)
        {
            properties = properties.Where(p => p.RentalType == rentalType);
        }

        if (query.Type is { } type)
        {
            properties = properties.Where(p => p.Type == type);
        }

        if (query.Search is { } search)
        {
            properties = properties.Where(p => p.Name.Contains(search) || p.Address.Contains(search));
        }

        if (query.MinPrice is { } minPrice)
        {
            properties = properties.Where(p => p.DayPrice >= minPrice);
        }

        if (query.MaxPrice is { } maxPrice)
        {
            properties = properties.Where(p => p.DayPrice <= maxPrice);
        }

        if (query.MinRooms is { } minRooms)
        {
            properties = properties.Where(p => p.Rooms >= minRooms);
        }

        int total = await properties.CountAsync(ct);
        var items = await properties
            .OrderBy(p => p.DayPrice)
            .ThenBy(p => p.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Property>(items, query.Page, query.PageSize, total);
    }

    public Task<Property?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Properties.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Property property, CancellationToken ct = default) =>
        await db.Properties.AddAsync(property, ct);

    public void Remove(Property property) => db.Properties.Remove(property);
}

public class ClientRepository(AppDbContext db) : IClientRepository
{
    public async Task<IReadOnlyList<Client>> SearchAsync(string? search, CancellationToken ct = default)
    {
        IQueryable<Client> clients = db.Clients.AsNoTracking();

        if (search is not null)
        {
            clients = clients.Where(c =>
                c.LastName.Contains(search) || c.FirstName.Contains(search) ||
                c.Email.Contains(search) || c.PassportNumber.Contains(search));
        }

        return await clients.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).Take(50).ToListAsync(ct);
    }

    public Task<Client?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> PassportExistsAsync(string passportNumber, CancellationToken ct = default) =>
        db.Clients.AnyAsync(c => c.PassportNumber == passportNumber, ct);

    public async Task AddAsync(Client client, CancellationToken ct = default) =>
        await db.Clients.AddAsync(client, ct);
}

public class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Email == email, ct);
}

public class BookingRepository(AppDbContext db) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Bookings
            .Include(b => b.Property)
            .Include(b => b.Client)
            .Include(b => b.Employee)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<Booking>> GetOverlappingAsync(int propertyId, DateOnly start, DateOnly end, CancellationToken ct = default)
    {
        // Same rule as RentalRules.Overlaps: [start, end) intersects [b.StartDate, b.EndDate).
        return await db.Bookings.AsNoTracking()
            .Where(b => b.PropertyId == propertyId
                        && b.Status == BookingStatus.Active
                        && b.StartDate < end
                        && start < b.EndDate)
            .OrderBy(b => b.StartDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> ListAsync(int? propertyId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        IQueryable<Booking> bookings = db.Bookings.AsNoTracking()
            .Include(b => b.Property)
            .Include(b => b.Client)
            .Include(b => b.Employee);

        if (propertyId is { } id)
        {
            bookings = bookings.Where(b => b.PropertyId == id);
        }

        if (from is { } fromDate)
        {
            bookings = bookings.Where(b => b.StartDate >= fromDate);
        }

        if (to is { } toDate)
        {
            bookings = bookings.Where(b => b.StartDate <= toDate);
        }

        return await bookings.OrderByDescending(b => b.StartDate).ThenBy(b => b.Id).ToListAsync(ct);
    }

    public Task<bool> AnyForPropertyAsync(int propertyId, CancellationToken ct = default) =>
        db.Bookings.AnyAsync(b => b.PropertyId == propertyId, ct);

    public async Task AddAsync(Booking booking, CancellationToken ct = default) =>
        await db.Bookings.AddAsync(booking, ct);
}
