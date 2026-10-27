using System.ComponentModel.DataAnnotations;
using AcmHouse.Domain.Entities;

namespace AcmHouse.Application.Dtos;

// ---- Auth ----

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string Role, string FullName);

// ---- Properties ----

public record PropertyDto(
    int Id,
    string Name,
    PropertyType Type,
    RentalType RentalType,
    string Address,
    string? FlatNumber,
    int Rooms,
    decimal Area,
    decimal DayPrice);

public record SavePropertyRequest(
    [Required, StringLength(100)] string Name,
    [EnumDataType(typeof(PropertyType))] PropertyType Type,
    [EnumDataType(typeof(RentalType))] RentalType RentalType,
    [Required, StringLength(200)] string Address,
    [StringLength(20)] string? FlatNumber,
    [Range(0, 50)] int Rooms,
    [Range(1, 100000)] decimal Area,
    [Range(1, 1000000)] decimal DayPrice);

public record AvailabilityDto(int PropertyId, DateOnly From, DateOnly To, bool IsAvailable, decimal? TotalPrice);

// ---- Clients ----

public record ClientDto(int Id, string FirstName, string LastName, string Email, string Phone, string PassportNumber);

public record CreateClientRequest(
    [Required, StringLength(50)] string FirstName,
    [Required, StringLength(50)] string LastName,
    [Required, EmailAddress, StringLength(100)] string Email,
    [Required, Phone, StringLength(30)] string Phone,
    [Required, RegularExpression("^[A-Z]{2}[A-Z0-9]{6,}$",
        ErrorMessage = "Passport number: two capital letters and the number, e.g. MP1234567.")]
    string PassportNumber);

// ---- Bookings ----

public record CreateBookingRequest(
    [Range(1, int.MaxValue)] int PropertyId,
    [Range(1, int.MaxValue)] int ClientId,
    DateOnly StartDate,
    DateOnly EndDate);

public record BookingDto(
    int Id,
    int PropertyId,
    string PropertyName,
    int ClientId,
    string ClientName,
    int EmployeeId,
    string EmployeeName,
    DateOnly StartDate,
    DateOnly EndDate,
    int Days,
    decimal TotalPrice,
    BookingStatus Status);

// ---- Reports ----

public record EmployeeRevenueDto(int EmployeeId, string EmployeeName, int Bookings, decimal Revenue, decimal Profit);

public record RevenueReportDto(DateOnly From, DateOnly To, int Bookings, decimal Revenue, IReadOnlyList<EmployeeRevenueDto> ByEmployee);
