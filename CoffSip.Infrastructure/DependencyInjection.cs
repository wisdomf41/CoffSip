
using CoffSip.Application.MenuItems;
using CoffSip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoffSip.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("CoffSipDatabase")
            ?? throw new InvalidOperationException(
                "CoffSip database connection string is missing.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();

        return services;
    }
}
