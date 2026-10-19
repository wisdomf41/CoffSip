using CoffSip.Application.MenuItems;
using CoffSip.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffSip.Infrastructure.Data;

// Implements menu persistence using EF Core and PostgreSQL.
public sealed class MenuItemRepository : IMenuItemRepository
{
    private readonly AppDbContext _db;

    public MenuItemRepository(AppDbContext db) => _db = db;

    public Task<List<MenuItem>> ListActiveAsync(CancellationToken ct) =>
        _db.MenuItems.AsNoTracking()
            .Where(x => !x.IsArchived)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public Task<MenuItem?> GetActiveAsync(Guid id, CancellationToken ct) =>
        _db.MenuItems.FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived, ct);

    public Task<MenuItem?> GetAnyAsync(Guid id, CancellationToken ct) =>
        _db.MenuItems.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(MenuItem item, CancellationToken ct)
    {
        await _db.MenuItems.AddAsync(item, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}
