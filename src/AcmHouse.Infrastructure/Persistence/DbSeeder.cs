using AcmHouse.Application.Abstractions;
using AcmHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcmHouse.Infrastructure.Persistence;

/// <summary>Fills an empty database with fictional demo data.</summary>
public static class DbSeeder
{
    public const string DemoAdminEmail = "admin@acmhouse.by";
    public const string DemoAdminPassword = "Admin1234";
    public const string DemoAgentEmail = "ivan.petrov@acmhouse.by";
    public const string DemoAgentPassword = "Demo1234";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        if (await db.Employees.AnyAsync(ct))
        {
            return;
        }

        db.Employees.AddRange(
            new Employee { FirstName = "Admin", LastName = "Manager", Email = DemoAdminEmail, PasswordHash = hasher.Hash(DemoAdminPassword), Role = Roles.Admin },
            new Employee { FirstName = "Иван", LastName = "Петров", Email = DemoAgentEmail, PasswordHash = hasher.Hash(DemoAgentPassword), Role = Roles.Agent },
            new Employee { FirstName = "Анна", LastName = "Ковалёва", Email = "anna.kovaleva@acmhouse.by", PasswordHash = hasher.Hash(DemoAgentPassword), Role = Roles.Agent });

        db.Properties.AddRange(
            new Property { Name = "Квартира в центре", Type = PropertyType.Apartment, RentalType = RentalType.Residential, Address = "пр-т Независимости 10", FlatNumber = "12", Rooms = 2, Area = 54.5m, DayPrice = 120m },
            new Property { Name = "Уютная квартира", Type = PropertyType.Apartment, RentalType = RentalType.Residential, Address = "ул. Немига 5", FlatNumber = "34", Rooms = 1, Area = 38m, DayPrice = 85m },
            new Property { Name = "Коттедж около Лебяжего", Type = PropertyType.Cottage, RentalType = RentalType.Residential, Address = "ул. Лебяжья 3", Rooms = 5, Area = 180m, DayPrice = 300m },
            new Property { Name = "Стильный дом", Type = PropertyType.House, RentalType = RentalType.Residential, Address = "ул. Революционная 67", Rooms = 4, Area = 140m, DayPrice = 250m },
            new Property { Name = "Офис на Немиге", Type = PropertyType.Office, RentalType = RentalType.Commercial, Address = "ул. Козлова 82", FlatNumber = "3", Rooms = 10, Area = 240m, DayPrice = 180m },
            new Property { Name = "Склад в Уручье", Type = PropertyType.Warehouse, RentalType = RentalType.Commercial, Address = "ул. Уручская 21", Rooms = 0, Area = 600m, DayPrice = 220m });

        db.Clients.AddRange(
            new Client { FirstName = "Олег", LastName = "Сидоренко", Email = "sidorenko@example.com", Phone = "+375 29 000-00-01", PassportNumber = "MP4567890" },
            new Client { FirstName = "Дмитрий", LastName = "Ермолаев", Email = "ermolaev@example.com", Phone = "+375 29 000-00-02", PassportNumber = "MP5678901" });

        await db.SaveChangesAsync(ct);
    }
}
