using AcmHouse.Application.Abstractions;
using AcmHouse.Infrastructure.Persistence;

namespace AcmHouse.Api.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// Creates the database if it does not exist and fills it with demo data.
    /// Runs only when Database:Initialize is true (on by default in Development).
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue("Database:Initialize", false))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db, hasher);
    }
}
