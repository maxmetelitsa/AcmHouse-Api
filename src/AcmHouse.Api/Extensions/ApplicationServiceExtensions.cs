using AcmHouse.Application.Abstractions;
using AcmHouse.Application.Services;

namespace AcmHouse.Api.Extensions;

public static class ApplicationServiceExtensions
{
    /// <summary>Registers the application services (business logic).</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<AuthService>();
        services.AddScoped<PropertyService>();
        services.AddScoped<ClientService>();
        services.AddScoped<BookingService>();
        services.AddScoped<ReportService>();
        return services;
    }
}
