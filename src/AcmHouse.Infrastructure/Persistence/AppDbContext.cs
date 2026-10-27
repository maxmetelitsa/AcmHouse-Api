using AcmHouse.Application.Abstractions;
using AcmHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcmHouse.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Property> Properties => Set<Property>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken ct)
    {
        await SaveChangesAsync(ct);
    }
}
