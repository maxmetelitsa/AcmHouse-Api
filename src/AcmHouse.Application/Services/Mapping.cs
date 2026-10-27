using AcmHouse.Application.Dtos;
using AcmHouse.Domain.Entities;
using AcmHouse.Domain.Rules;

namespace AcmHouse.Application.Services;

internal static class Mapping
{
    public static PropertyDto ToDto(this Property p) =>
        new(p.Id, p.Name, p.Type, p.RentalType, p.Address, p.FlatNumber, p.Rooms, p.Area, p.DayPrice);

    public static ClientDto ToDto(this Client c) =>
        new(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.PassportNumber);

    public static BookingDto ToDto(this Booking b) =>
        new(b.Id,
            b.PropertyId,
            b.Property?.Name ?? string.Empty,
            b.ClientId,
            b.Client is null ? string.Empty : $"{b.Client.FirstName} {b.Client.LastName}",
            b.EmployeeId,
            b.Employee is null ? string.Empty : $"{b.Employee.FirstName} {b.Employee.LastName}",
            b.StartDate,
            b.EndDate,
            RentalRules.CountDays(b.StartDate, b.EndDate),
            b.TotalPrice,
            b.Status);

    public static void Apply(this Property p, SavePropertyRequest r)
    {
        p.Name = r.Name.Trim();
        p.Type = r.Type;
        p.RentalType = r.RentalType;
        p.Address = r.Address.Trim();
        p.FlatNumber = string.IsNullOrWhiteSpace(r.FlatNumber) ? null : r.FlatNumber.Trim();
        p.Rooms = r.Rooms;
        p.Area = r.Area;
        p.DayPrice = r.DayPrice;
    }
}
