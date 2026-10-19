using CoffSip.Domain.Entities;

namespace CoffSip.Application.MenuItems;

// Defines the public request and response models for the menu API.
public sealed record CreateMenuItemRequest(
    string? Name,
    string? Description,
    string? Category,
    decimal? Price,
    bool? IsAvailable);

public sealed record UpdateMenuItemRequest(
    string? Name,
    string? Description,
    string? Category,
    decimal? Price,
    bool? IsAvailable);

public sealed record MenuItemResponse(
    Guid Id,
    string Name,
    string? Description,
    string Category,
    decimal Price,
    bool IsAvailable,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static MenuItemResponse FromEntity(MenuItem item) => new(
        item.Id, item.Name, item.Description, item.Category,
        item.Price, item.IsAvailable, item.IsArchived,
        item.CreatedAt, item.UpdatedAt);
}
