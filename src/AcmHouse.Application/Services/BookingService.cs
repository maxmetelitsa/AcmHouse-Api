using AcmHouse.Application.Abstractions;
using AcmHouse.Application.Dtos;
using AcmHouse.Application.Exceptions;
using AcmHouse.Domain.Entities;
using AcmHouse.Domain.Rules;

namespace AcmHouse.Application.Services;

public class BookingService(
    IBookingRepository bookings,
    IPropertyRepository properties,
    IClientRepository clients,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<BookingDto> CreateAsync(CreateBookingRequest request, int employeeId, CancellationToken ct = default)
    {
        string? periodError = RentalRules.ValidatePeriod(request.StartDate, request.EndDate, clock.Today);
        if (periodError is not null)
        {
            throw new BusinessRuleException(periodError);
        }

        var property = await properties.GetByIdAsync(request.PropertyId, ct)
            ?? throw new NotFoundException($"Property {request.PropertyId} was not found.");

        var client = await clients.GetByIdAsync(request.ClientId, ct)
            ?? throw new NotFoundException($"Client {request.ClientId} was not found.");

        var overlapping = await bookings.GetOverlappingAsync(property.Id, request.StartDate, request.EndDate, ct);
        if (overlapping.Count > 0)
        {
            var first = overlapping[0];
            throw new ConflictException(
                $"The property is already booked from {first.StartDate:yyyy-MM-dd} to {first.EndDate:yyyy-MM-dd}.");
        }

        decimal total = RentalRules.CalculateTotal(request.StartDate, request.EndDate, property.DayPrice);

        var booking = new Booking
        {
            PropertyId = property.Id,
            Property = property,
            ClientId = client.Id,
            Client = client,
            EmployeeId = employeeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalPrice = total,
            AgentProfit = RentalRules.CalculateAgentProfit(total),
            Status = BookingStatus.Active,
            CreatedAtUtc = clock.UtcNow
        };

        await bookings.AddAsync(booking, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Reload so the response carries the employee's name as well.
        var saved = await bookings.GetByIdAsync(booking.Id, ct) ?? booking;
        return saved.ToDto();
    }

    public async Task<BookingDto> GetAsync(int id, CancellationToken ct = default)
    {
        return (await FindAsync(id, ct)).ToDto();
    }

    public async Task<IReadOnlyList<BookingDto>> ListAsync(int? propertyId, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var found = await bookings.ListAsync(propertyId, from, to, ct);
        return found.Select(b => b.ToDto()).ToList();
    }

    /// <summary>Cancels a booking. Agents can cancel only their own bookings; administrators can cancel any.</summary>
    public async Task CancelAsync(int id, int employeeId, bool isAdmin, CancellationToken ct = default)
    {
        var booking = await FindAsync(id, ct);

        if (!isAdmin && booking.EmployeeId != employeeId)
        {
            throw new ForbiddenException("Only the agent who signed the booking or an administrator can cancel it.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new ConflictException("The booking is already cancelled.");
        }

        if (booking.StartDate <= clock.Today)
        {
            throw new BusinessRuleException("A booking that has already started cannot be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Booking> FindAsync(int id, CancellationToken ct)
    {
        return await bookings.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Booking {id} was not found.");
    }
}
