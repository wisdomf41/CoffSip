using CoffSip.Domain.Entities;

namespace CoffSip.Application.MenuItems;

// Isolates application logic from the PostgreSQL persistence implementation.
public interface IMenuItemRepository
{
    Task<List<MenuItem>> ListActiveAsync(CancellationToken cancellationToken);
    Task<MenuItem?> GetActiveAsync(Guid id, CancellationToken cancellationToken);
    Task<MenuItem?> GetAnyAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(MenuItem item, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
