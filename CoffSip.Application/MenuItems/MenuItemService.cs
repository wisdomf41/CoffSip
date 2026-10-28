
using CoffSip.Domain.Entities;

namespace CoffSip.Application.MenuItems
{
    // Enforce operator ownership inside the application service before database writes.
    public sealed class MenuItemService
    {
        private readonly IMenuItemRepository _repository;

        public MenuItemService(IMenuItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<MenuItemResponse>> ListAsync(
            int page,
            int pageSize,
            CancellationToken ct)
        {
            var items = await _repository.ListActiveAsync(page, pageSize, ct);

            return items
                .Select(MenuItemResponse.FromEntity)
                .ToList();
        }

        public async Task<IReadOnlyList<MenuItemResponse>> ListMineAsync(
            Guid ownerId,
            CancellationToken ct)
        {
            var items = await _repository.ListOwnedActiveAsync(
                ownerId, ct);

            return items
                .Select(MenuItemResponse.FromEntity)
                .ToList();
        }

        public async Task<MenuItemResponse?> GetAsync(
            Guid id,
            CancellationToken ct)
        {
            var item = await _repository.GetActiveAsync(id, ct);

            return item is null
                ? null
                : MenuItemResponse.FromEntity(item);
        }

        public async Task<MenuItemResponse> CreateAsync(
            CreateMenuItemRequest request,
            Guid ownerId,
            CancellationToken ct)
        {
            var (name, description, category, price, available) =
                Validate(
                    request.Name,
                    request.Description,
                    request.Category,
                    request.Price,
                    request.IsAvailable,
                    requireAvailability: false);

            var item = new MenuItem
            {
                OwnerId = ownerId,
                Name = name,
                Description = description,
                Category = category,
                Price = price,
                IsAvailable = available
            };

            await _repository.AddAsync(item, ct);
            await _repository.SaveChangesAsync(ct);

            return MenuItemResponse.FromEntity(item);
        }

        public async Task<MenuItemResponse?> UpdateAsync(
            Guid id,
            UpdateMenuItemRequest request,
            Guid ownerId,
            CancellationToken ct)
        {
            var item = await _repository.GetActiveAsync(id, ct);

            if (item is null)
            {
                return null;
            }

            if (item.OwnerId != ownerId)
            {
                throw new MenuItemOwnershipException();
            }

            var (name, description, category, price, available) =
                Validate(
                    request.Name,
                    request.Description,
                    request.Category,
                    request.Price,
                    request.IsAvailable,
                    requireAvailability: true);

            item.Name = name;
            item.Description = description;
            item.Category = category;
            item.Price = price;
            item.IsAvailable = available;
            item.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveChangesAsync(ct);

            return MenuItemResponse.FromEntity(item);
        }

        public async Task<bool> ArchiveAsync(
            Guid id,
            Guid ownerId,
            CancellationToken ct)
        {
            var item = await _repository.GetAnyAsync(id, ct);

            if (item is null)
            {
                return false;
            }

            if (item.OwnerId != ownerId)
            {
                throw new MenuItemOwnershipException();
            }

            if (!item.IsArchived)
            {
                item.IsArchived = true;
                item.UpdatedAt = DateTime.UtcNow;

                await _repository.SaveChangesAsync(ct);
            }

            return true;
        }

        private static (
            string Name,
            string? Description,
            string Category,
            decimal Price,
            bool IsAvailable
        ) Validate(
            string? name,
            string? description,
            string? category,
            decimal? price,
            bool? available,
            bool requireAvailability)
        {
            var errors = new Dictionary<string, string[]>();

            var cleanName = name?.Trim() ?? string.Empty;
            var cleanCategory = category?.Trim() ?? string.Empty;
            var cleanDescription = description?.Trim();

            if (cleanName.Length is < 1 or > 120)
            {
                errors["name"] =
                [
                    "Name is required and must contain at most 120 characters."
                ];
            }

            if (cleanCategory.Length is < 1 or > 60)
            {
                errors["category"] =
                [
                    "Category is required and must contain at most 60 characters."
                ];
            }

            if (cleanDescription?.Length > 500)
            {
                errors["description"] =
                [
                    "Description must contain at most 500 characters."
                ];
            }

            if (price is null ||
                price <= 0 ||
                price > 9999999999.99m ||
                decimal.Round(price.Value, 2) != price.Value)
            {
                errors["price"] =
                [
                    "Price must be positive, fit numeric(12,2), and have at most 2 decimal places."
                ];
            }

            if (requireAvailability && available is null)
            {
                errors["isAvailable"] =
                [
                    "Availability is required when updating a menu item."
                ];
            }

            if (errors.Count > 0)
            {
                throw new MenuItemValidationException(errors);
            }

            return (
                cleanName,
                string.IsNullOrWhiteSpace(cleanDescription)
                    ? null
                    : cleanDescription,
                cleanCategory,
                price!.Value,
                available ?? true);
        }
    }
}
