using CoffSip.Application.MenuItems;
using Microsoft.AspNetCore.Mvc;

namespace CoffSip.Api.Controllers;

// Implements the five documented MenuItem REST operations.
[ApiController]
[Route("api/menu-items")]
public sealed class MenuItemsController : ControllerBase
{
    private readonly MenuItemService _service;

    public MenuItemsController(MenuItemService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await _service.ListAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var parsedId)) return InvalidId();
        var item = await _service.GetAsync(parsedId, ct);
        return item is null ? MissingItem() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemRequest request, CancellationToken ct)
    {
        try
        {
            var item = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        catch (MenuItemValidationException ex)
        {
            return InvalidInput(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMenuItemRequest request,
        CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var parsedId)) return InvalidId();
        try
        {
            var item = await _service.UpdateAsync(parsedId, request, ct);
            return item is null ? MissingItem() : Ok(item);
        }
        catch (MenuItemValidationException ex)
        {
            return InvalidInput(ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var parsedId)) return InvalidId();
        return await _service.ArchiveAsync(parsedId, ct) ? NoContent() : MissingItem();
    }

    private IActionResult InvalidId() => ValidationProblem(
        new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["id"] = ["The id must be a valid UUID."]
        })
        { Status = 400, Title = "One or more validation errors occurred." });

    private IActionResult InvalidInput(MenuItemValidationException ex) => ValidationProblem(
        new ValidationProblemDetails(ex.Errors.ToDictionary(x => x.Key, x => x.Value))
        { Status = 400, Title = "One or more validation errors occurred." });

    private IActionResult MissingItem() => NotFound(new ProblemDetails
    {
        Status = 404,
        Title = "Menu item not found.",
        Detail = "No active menu item matches the supplied ID."
    });
}
