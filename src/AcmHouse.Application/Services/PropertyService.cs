using AcmHouse.Application.Abstractions;
using AcmHouse.Application.Dtos;
using AcmHouse.Application.Exceptions;
using AcmHouse.Domain.Entities;
using AcmHouse.Domain.Rules;

namespace AcmHouse.Application.Services;

public class PropertyService(IPropertyRepository properties, IBookingRepository bookings, IUnitOfWork unitOfWork, IClock clock)
{
    public const int MaxPageSize = 100;

    public async Task<PagedResult<PropertyDto>> SearchAsync(PropertyQuery query, CancellationToken ct = default)
    {
        if (query.MinPrice is not null && query.MaxPrice is not null && query.MinPrice > query.MaxPrice)
        {
            throw new BusinessRuleException("minPrice cannot be greater than maxPrice.");
        }

        var normalized = query with
        {
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, MaxPageSize),
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim()
        };

        var page = await properties.SearchAsync(normalized, ct);
        return new PagedResult<PropertyDto>(page.Items.Select(p => p.ToDto()).ToList(), page.Page, page.PageSize, page.TotalCount);
    }

    public async Task<PropertyDto> GetAsync(int id, CancellationToken ct = default)
    {
        return (await FindAsync(id, ct)).ToDto();
    }

    public async Task<PropertyDto> CreateAsync(SavePropertyRequest request, CancellationToken ct = default)
    {
        var property = new Property();
        property.Apply(request);

        await properties.AddAsync(property, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return property.ToDto();
    }

    public async Task<PropertyDto> UpdateAsync(int id, SavePropertyRequest request, CancellationToken ct = default)
    {
        var property = await FindAsync(id, ct);
        property.Apply(request);

        await unitOfWork.SaveChangesAsync(ct);
        return property.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var property = await FindAsync(id, ct);

        // Contracts must stay in the history, so a property that has ever been rented cannot be deleted.
        if (await bookings.AnyForPropertyAsync(id, ct))
        {
            throw new ConflictException("The property has bookings and cannot be deleted.");
        }

        properties.Remove(property);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<AvailabilityDto> CheckAvailabilityAsync(int id, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var property = await FindAsync(id, ct);

        string? periodError = RentalRules.ValidatePeriod(from, to, clock.Today);
        if (periodError is not null)
        {
            throw new BusinessRuleException(periodError);
        }

        var overlapping = await bookings.GetOverlappingAsync(id, from, to, ct);
        bool isAvailable = overlapping.Count == 0;
        decimal? total = isAvailable ? RentalRules.CalculateTotal(from, to, property.DayPrice) : null;

        return new AvailabilityDto(id, from, to, isAvailable, total);
    }

    private async Task<Property> FindAsync(int id, CancellationToken ct)
    {
        return await properties.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Property {id} was not found.");
    }
}
