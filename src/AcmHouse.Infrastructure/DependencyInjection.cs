using AcmHouse.Application.Abstractions;
using AcmHouse.Infrastructure.Persistence;
using AcmHouse.Infrastructure.Repositories;
using AcmHouse.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AcmHouse.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers EF Core with SQL Server, the repositories and the password hasher.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        return services.AddRepositories();
    }

    /// <summary>Registers everything except the database provider (tests plug in their own).</summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        return services;
    }
}
