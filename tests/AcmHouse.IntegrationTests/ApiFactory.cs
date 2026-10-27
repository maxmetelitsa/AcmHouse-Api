using System.Net.Http.Headers;
using System.Net.Http.Json;
using AcmHouse.Application.Dtos;
using AcmHouse.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AcmHouse.IntegrationTests;

/// <summary>
/// Runs the whole API in memory: real controllers, JWT authentication and EF Core,
/// with the EF Core in-memory provider instead of SQL Server. Each factory gets its own database.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"acmhouse-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // appsettings.Testing.json provides the JWT key and turns on demo data.
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName)));
    }

    public async Task<HttpClient> CreateClientAsAsync(string email, string password)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(TestJson.Options);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    public Task<HttpClient> CreateAgentClientAsync() =>
        CreateClientAsAsync(DbSeeder.DemoAgentEmail, DbSeeder.DemoAgentPassword);

    public Task<HttpClient> CreateAdminClientAsync() =>
        CreateClientAsAsync(DbSeeder.DemoAdminEmail, DbSeeder.DemoAdminPassword);
}

public static class TestJson
{
    public static readonly System.Text.Json.JsonSerializerOptions Options = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
}
