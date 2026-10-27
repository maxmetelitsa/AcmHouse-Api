using AcmHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcmHouse.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Address).HasMaxLength(200).IsRequired();
        builder.Property(p => p.FlatNumber).HasMaxLength(20);
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.RentalType).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Area).HasPrecision(10, 2);
        builder.Property(p => p.DayPrice).HasPrecision(12, 2);

        builder.HasIndex(p => new { p.RentalType, p.DayPrice });
    }
}

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.Property(c => c.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(30).IsRequired();
        builder.Property(c => c.PassportNumber).HasMaxLength(20).IsRequired();

        builder.HasIndex(c => c.PassportNumber).IsUnique();
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(100).IsRequired();
        builder.Property(e => e.PasswordHash).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Role).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => e.Email).IsUnique();
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.TotalPrice).HasPrecision(12, 2);
        builder.Property(b => b.AgentProfit).HasPrecision(12, 2);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);

        // Bookings are contracts: deleting a property, client or employee must not delete them.
        builder.HasOne(b => b.Property).WithMany(p => p.Bookings).HasForeignKey(b => b.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Client).WithMany(c => c.Bookings).HasForeignKey(b => b.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Employee).WithMany(e => e.Bookings).HasForeignKey(b => b.EmployeeId).OnDelete(DeleteBehavior.Restrict);

        // Speeds up the availability check, which runs on every booking.
        builder.HasIndex(b => new { b.PropertyId, b.StartDate, b.EndDate });
    }
}
