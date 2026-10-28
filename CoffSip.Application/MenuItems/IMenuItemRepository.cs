using CoffSip.Domain.Entities;

namespace CoffSip.Application.MenuItems
{
        // Add a repository query for menu items owned by one operator.
    public interface IMenuItemRepository
    {
        Task<List<MenuItem>> ListActiveAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken);

        Task<List<MenuItem>> ListOwnedActiveAsync(
            Guid ownerId,
            CancellationToken cancellationToken);

        Task<MenuItem?> GetActiveAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task<MenuItem?> GetAnyAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task AddAsync(
            MenuItem item,
            CancellationToken cancellationToken);

        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }



}