using CoffSip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffSip.Infrastructure.Data;

// Creates the demo schema and seeds fictional menu items when explicitly enabled.
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);
        if (await db.MenuItems.AnyAsync(ct)) return;

        db.MenuItems.AddRange(
            new MenuItem
            {
                Name = "Cappuccino",
                Category = "Coffee",
                Description = "Espresso with steamed milk",
                Price = 4.50m,
                IsAvailable = true
            },
            new MenuItem
            {
                Name = "Espresso",
                Category = "Coffee",
                Description = "A single shot of espresso",
                Price = 3.00m,
                IsAvailable = true
            },
            new MenuItem
            {
                Name = "Croissant",
                Category = "Pastry",
                Description = "Butter croissant",
                Price = 3.25m,
                IsAvailable = true
            });
        await db.SaveChangesAsync(ct);
    }
}
