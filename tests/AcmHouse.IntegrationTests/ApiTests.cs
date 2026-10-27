using System.Net;
using System.Net.Http.Json;
using AcmHouse.Application.Abstractions;
using AcmHouse.Application.Dtos;
using AcmHouse.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AcmHouse.IntegrationTests;

public class ApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;

    public ApiTests(ApiFactory factory) => this.factory = factory;

    // Far enough in the future that the tests do not depend on today's date.
    private static DateOnly Day(int offset) => DateOnly.FromDateTime(DateTime.Today).AddDays(offset);

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Catalog_IsPublicAndFiltersByRentalType()
    {
        var page = await factory.CreateClient()
            .GetFromJsonAsync<PagedResult<PropertyDto>>("/api/properties?rentalType=Commercial", TestJson.Options);

        Assert.NotNull(page);
        Assert.NotEmpty(page!.Items);
        Assert.All(page.Items, p => Assert.Equal(RentalType.Commercial, p.RentalType));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@acmhouse.by", "wrong"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Clients_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/clients");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProperty_AsAgent_Returns403()
    {
        var agent = await factory.CreateAgentClientAsync();

        var response = await agent.PostAsJsonAsync("/api/properties", NewProperty(), TestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProperty_AsAdmin_Returns201WithLocation()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/properties", NewProperty(), TestJson.Options);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Booking_FullFlow_CreateConflictCancelAndRebook()
    {
        var agent = await factory.CreateAgentClientAsync();
        var admin = await factory.CreateAdminClientAsync();

        // A fresh property, so other tests cannot have booked it.
        var created = await admin.PostAsJsonAsync("/api/properties", NewProperty(), TestJson.Options);
        var property = await created.Content.ReadFromJsonAsync<PropertyDto>(TestJson.Options);

        var request = new CreateBookingRequest(property!.Id, 1, Day(30), Day(33));

        var first = await agent.PostAsJsonAsync("/api/bookings", request, TestJson.Options);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var booking = await first.Content.ReadFromJsonAsync<BookingDto>(TestJson.Options);
        Assert.Equal(3, booking!.Days);
        Assert.Equal(3 * property.DayPrice, booking.TotalPrice);

        var overlapping = await agent.PostAsJsonAsync("/api/bookings", request with { StartDate = Day(32), EndDate = Day(35) }, TestJson.Options);
        Assert.Equal(HttpStatusCode.Conflict, overlapping.StatusCode);
        var problem = await overlapping.Content.ReadFromJsonAsync<ProblemDetails>(TestJson.Options);
        Assert.Contains("already booked", problem!.Detail);

        var cancel = await agent.PostAsync($"/api/bookings/{booking.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);

        var rebooked = await agent.PostAsJsonAsync("/api/bookings", request, TestJson.Options);
        Assert.Equal(HttpStatusCode.Created, rebooked.StatusCode);
    }

    [Fact]
    public async Task Booking_InThePast_Returns400()
    {
        var agent = await factory.CreateAgentClientAsync();

        var response = await agent.PostAsJsonAsync("/api/bookings", new CreateBookingRequest(1, 1, Day(-5), Day(-2)), TestJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateClient_WithInvalidPassport_ReturnsValidationErrors()
    {
        var agent = await factory.CreateAgentClientAsync();

        var response = await agent.PostAsJsonAsync("/api/clients",
            new CreateClientRequest("Елена", "Лукашевич", "lukashevich@example.com", "+375 33 000-00-03", "mp12"), TestJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJson.Options);
        Assert.True(problem!.Errors.ContainsKey("PassportNumber"));
    }

    [Fact]
    public async Task RevenueReport_AsAgent_Returns403()
    {
        var agent = await factory.CreateAgentClientAsync();

        var response = await agent.GetAsync($"/api/reports/revenue?from={Day(0):yyyy-MM-dd}&to={Day(60):yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static SavePropertyRequest NewProperty() =>
        new("Тестовая квартира", PropertyType.Apartment, RentalType.Residential, "ул. Тестовая 1", null, 2, 50m, 100m);
}
