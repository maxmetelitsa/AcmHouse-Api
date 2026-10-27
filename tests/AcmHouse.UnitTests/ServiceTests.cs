using AcmHouse.Application.Abstractions;
using AcmHouse.Application.Dtos;
using AcmHouse.Application.Exceptions;
using AcmHouse.Application.Services;
using AcmHouse.Domain.Entities;
using AcmHouse.UnitTests.Fakes;
using Xunit;

namespace AcmHouse.UnitTests;

public class AuthServiceTests
{
    private readonly TestData data = new();

    private AuthService CreateService() => new(data.Employees, data.Hasher, new FakeTokenService());

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndRole()
    {
        var response = await CreateService().LoginAsync(new LoginRequest(" admin@acmhouse.by ", "Admin1234"));

        Assert.Equal("token-for-1", response.AccessToken);
        Assert.Equal(Roles.Admin, response.Role);
    }

    [Theory]
    [InlineData("admin@acmhouse.by", "wrong")]
    [InlineData("nobody@acmhouse.by", "Admin1234")]
    public async Task Login_WithWrongEmailOrPassword_GivesTheSameError(string email, string password)
    {
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => CreateService().LoginAsync(new LoginRequest(email, password)));
    }
}

public class PropertyServiceTests
{
    private readonly TestData data = new();

    private PropertyService CreateService() => new(data.Properties, data.Bookings, data.Store, data.Clock);

    private static SavePropertyRequest Request(decimal price = 90m) =>
        new("Новая квартира", PropertyType.Apartment, RentalType.Residential, " ул. Сурганова 1 ", " ", 1, 30m, price);

    [Fact]
    public async Task Search_FiltersByRentalTypeAndSortsByPrice()
    {
        var page = await CreateService().SearchAsync(new PropertyQuery(RentalType: RentalType.Residential));

        Assert.Equal(new[] { "Квартира в центре", "Стильный дом" }, page.Items.Select(p => p.Name));
        Assert.Equal(2, page.TotalCount);
    }

    [Fact]
    public async Task Search_ClampsPageSizeAndFiltersByPrice()
    {
        var page = await CreateService().SearchAsync(new PropertyQuery(MinPrice: 150m, PageSize: 1000));

        Assert.Equal(PropertyService.MaxPageSize, page.PageSize);
        Assert.Equal(new[] { 180m, 250m }, page.Items.Select(p => p.DayPrice));
    }

    [Fact]
    public async Task Search_RejectsMinPriceAboveMaxPrice()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().SearchAsync(new PropertyQuery(MinPrice: 200m, MaxPrice: 100m)));
    }

    [Fact]
    public async Task Create_TrimsTextAndSaves()
    {
        var created = await CreateService().CreateAsync(Request());

        Assert.Equal(4, created.Id);
        Assert.Equal("ул. Сурганова 1", created.Address);
        Assert.Null(created.FlatNumber);
    }

    [Fact]
    public async Task Update_UnknownProperty_IsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().UpdateAsync(99, Request()));
    }

    [Fact]
    public async Task Delete_PropertyWithBookings_IsAConflict()
    {
        data.AddBooking(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 12));

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().DeleteAsync(data.Flat.Id));
        Assert.Contains(data.Flat, data.Store.Properties);
    }

    [Fact]
    public async Task Delete_PropertyWithoutBookings_RemovesIt()
    {
        await CreateService().DeleteAsync(data.Office.Id);

        Assert.DoesNotContain(data.Office, data.Store.Properties);
    }

    [Fact]
    public async Task CheckAvailability_ReturnsPriceWhenFree()
    {
        var result = await CreateService().CheckAvailabilityAsync(data.Flat.Id, new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 13));

        Assert.True(result.IsAvailable);
        Assert.Equal(360m, result.TotalPrice);
    }

    [Fact]
    public async Task CheckAvailability_ReportsTakenDates()
    {
        data.AddBooking(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 15));

        var result = await CreateService().CheckAvailabilityAsync(data.Flat.Id, new DateOnly(2026, 5, 12), new DateOnly(2026, 5, 14));

        Assert.False(result.IsAvailable);
        Assert.Null(result.TotalPrice);
    }
}

public class ClientServiceTests
{
    private readonly TestData data = new();

    private ClientService CreateService() => new(data.Clients, data.Store);

    [Fact]
    public async Task Create_RejectsDuplicatePassport()
    {
        var request = new CreateClientRequest("Олег", "Сидоренко", "other@example.com", "+375 29 111-11-11", "MP4567890");

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().CreateAsync(request));
    }

    [Fact]
    public async Task Search_MatchesNameIgnoringCase()
    {
        var found = await CreateService().SearchAsync("сидор");

        Assert.Equal("Сидоренко", Assert.Single(found).LastName);
    }
}

public class ReportServiceTests
{
    private readonly TestData data = new();

    [Fact]
    public async Task Revenue_SumsActiveBookingsPerEmployee()
    {
        data.AddBooking(new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 15));                                   // 600, Ivan
        data.AddBooking(new DateOnly(2026, 5, 20), new DateOnly(2026, 5, 22), employee: data.OtherAgent);       // 240, Anna
        data.AddBooking(new DateOnly(2026, 5, 25), new DateOnly(2026, 5, 27), BookingStatus.Cancelled);         // ignored
        data.AddBooking(new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 12));                                   // outside the period

        var report = await new ReportService(data.Bookings).GetRevenueAsync(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        Assert.Equal(2, report.Bookings);
        Assert.Equal(840m, report.Revenue);
        Assert.Equal(new[] { "Иван Петров", "Анна Ковалёва" }, report.ByEmployee.Select(e => e.EmployeeName));
        Assert.Equal(63m, report.ByEmployee[0].Profit);
    }

    [Fact]
    public async Task Revenue_RejectsReversedPeriod()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            new ReportService(data.Bookings).GetRevenueAsync(new DateOnly(2026, 5, 31), new DateOnly(2026, 5, 1)));
    }
}
