using AcmHouse.Application.Dtos;
using AcmHouse.Application.Exceptions;
using AcmHouse.Application.Services;
using AcmHouse.Domain.Entities;
using AcmHouse.UnitTests.Fakes;
using Xunit;

namespace AcmHouse.UnitTests;

public class BookingServiceTests
{
    private readonly TestData data = new();

    private BookingService CreateService() =>
        new(data.Bookings, data.Properties, data.Clients, data.Store, data.Clock);

    private static DateOnly Day(int day) => new(2026, 5, day);

    [Fact]
    public async Task Create_CalculatesPriceAndAgentProfit()
    {
        var booking = await CreateService().CreateAsync(new CreateBookingRequest(data.Flat.Id, data.Client.Id, Day(10), Day(15)), data.Agent.Id);

        Assert.Equal(5, booking.Days);
        Assert.Equal(600m, booking.TotalPrice);
        Assert.Equal("Иван Петров", booking.EmployeeName);
        Assert.Equal(63m, Assert.Single(data.Store.Bookings).AgentProfit);
        Assert.Equal(1, data.Store.SaveCalls);
    }

    [Fact]
    public async Task Create_RejectsOverlappingDates()
    {
        data.AddBooking(Day(10), Day(15));

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(new CreateBookingRequest(data.Flat.Id, data.Client.Id, Day(14), Day(18)), data.Agent.Id));

        Assert.Contains("2026-05-10", error.Message);
        Assert.Single(data.Store.Bookings);
    }

    [Fact]
    public async Task Create_AllowsCheckInOnTheCheckOutDayOfThePreviousStay()
    {
        data.AddBooking(Day(10), Day(15));

        await CreateService().CreateAsync(new CreateBookingRequest(data.Flat.Id, data.Client.Id, Day(15), Day(18)), data.Agent.Id);

        Assert.Equal(2, data.Store.Bookings.Count);
    }

    [Fact]
    public async Task Create_IgnoresCancelledBookings()
    {
        data.AddBooking(Day(10), Day(15), BookingStatus.Cancelled);

        await CreateService().CreateAsync(new CreateBookingRequest(data.Flat.Id, data.Client.Id, Day(10), Day(15)), data.Agent.Id);

        Assert.Equal(2, data.Store.Bookings.Count);
    }

    [Fact]
    public async Task Create_ChecksOnlyTheSameProperty()
    {
        data.AddBooking(Day(10), Day(15), property: data.House);

        await CreateService().CreateAsync(new CreateBookingRequest(data.Flat.Id, data.Client.Id, Day(10), Day(15)), data.Agent.Id);

        Assert.Equal(2, data.Store.Bookings.Count);
    }

    [Fact]
    public async Task Create_RejectsStartInThePast()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().CreateAsync(new CreateBookingRequest(data.Flat.Id, data.Client.Id, TestData.Today.AddDays(-1), Day(5)), data.Agent.Id));
    }

    [Theory]
    [InlineData(99, 1)]
    [InlineData(1, 99)]
    public async Task Create_ReturnsNotFoundForUnknownPropertyOrClient(int propertyId, int clientId)
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreateAsync(new CreateBookingRequest(propertyId, clientId, Day(10), Day(12)), data.Agent.Id));
    }

    [Fact]
    public async Task Cancel_ByTheSigningAgent_CancelsTheBooking()
    {
        var booking = data.AddBooking(Day(10), Day(15));

        await CreateService().CancelAsync(booking.Id, data.Agent.Id, isAdmin: false);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public async Task Cancel_ByAnotherAgent_IsForbidden()
    {
        var booking = data.AddBooking(Day(10), Day(15));

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().CancelAsync(booking.Id, data.OtherAgent.Id, isAdmin: false));
        Assert.Equal(BookingStatus.Active, booking.Status);
    }

    [Fact]
    public async Task Cancel_ByAdmin_IsAllowedForAnyBooking()
    {
        var booking = data.AddBooking(Day(10), Day(15));

        await CreateService().CancelAsync(booking.Id, data.Admin.Id, isAdmin: true);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public async Task Cancel_RejectsBookingThatHasStarted()
    {
        var booking = data.AddBooking(TestData.Today, Day(5));

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CancelAsync(booking.Id, data.Agent.Id, isAdmin: false));
    }

    [Fact]
    public async Task Cancel_Twice_IsAConflict()
    {
        var booking = data.AddBooking(Day(10), Day(15), BookingStatus.Cancelled);

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().CancelAsync(booking.Id, data.Agent.Id, isAdmin: false));
    }
}
