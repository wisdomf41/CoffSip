
using CoffSip.Application.MenuItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffSip.Api.Controllers
{
    // Enforce authentication and pass the JWT subject into ownership-aware menu operations.
    [ApiController]
    [Route("api/menu-items")]
    public sealed class MenuItemsController : ControllerBase
    {
        private readonly MenuItemService _service;

        public MenuItemsController(MenuItemService service)
        {
            _service = service;
        }

        // Accept optional pagination parameters with safe limits.
        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var currentPage = page ?? 1;
            var currentSize = pageSize ?? 10;

            if (currentPage < 1 ||
                currentPage > 10000 ||
                currentSize < 1 ||
                currentSize > 50)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Invalid pagination parameters."
                });
            }

            return Ok(await _service.ListAsync(
                currentPage, currentSize, ct));
        }

        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> Mine(CancellationToken ct)
        {
            var ownerId = GetCurrentUserId();

            if (ownerId is null)
            {
                return Unauthorized();
            }

            return Ok(await _service.ListMineAsync(
                ownerId.Value, ct));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            string id,
            CancellationToken ct)
        {
            if (!Guid.TryParse(id, out var parsedId))
            {
                return InvalidId();
            }

            var item = await _service.GetAsync(parsedId, ct);

            return item is null ? MissingItem() : Ok(item);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateMenuItemRequest request,
            CancellationToken ct)
        {
            var ownerId = GetCurrentUserId();

            if (ownerId is null)
            {
                return Unauthorized();
            }

            try
            {
                var item = await _service.CreateAsync(
                    request, ownerId.Value, ct);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = item.Id },
                    item);
            }
            catch (MenuItemValidationException ex)
            {
                return InvalidInput(ex);
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] UpdateMenuItemRequest request,
            CancellationToken ct)
        {
            if (!Guid.TryParse(id, out var parsedId))
            {
                return InvalidId();
            }

            var ownerId = GetCurrentUserId();

            if (ownerId is null)
            {
                return Unauthorized();
            }

            try
            {
                var item = await _service.UpdateAsync(
                    parsedId, request, ownerId.Value, ct);

                return item is null ? MissingItem() : Ok(item);
            }
            catch (MenuItemOwnershipException)
            {
                return OwnershipForbidden();
            }
            catch (MenuItemValidationException ex)
            {
                return InvalidInput(ex);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            string id,
            CancellationToken ct)
        {
            if (!Guid.TryParse(id, out var parsedId))
            {
                return InvalidId();
            }

            var ownerId = GetCurrentUserId();

            if (ownerId is null)
            {
                return Unauthorized();
            }

            try
            {
                var archived = await _service.ArchiveAsync(
                    parsedId, ownerId.Value, ct);

                return archived ? NoContent() : MissingItem();
            }
            catch (MenuItemOwnershipException)
            {
                return OwnershipForbidden();
            }
        }

        private Guid? GetCurrentUserId()
        {
            return Guid.TryParse(
                User.FindFirst("sub")?.Value,
                out var userId)
                    ? userId
                    : null;
        }

        private IActionResult OwnershipForbidden()
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "You do not own this menu item.");
        }

        private IActionResult InvalidId() => ValidationProblem(
            new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["id"] = ["The id must be a valid UUID."]
                })
            {
                Status = 400,
                Title = "One or more validation errors occurred."
            });

        private IActionResult InvalidInput(
            MenuItemValidationException ex) => ValidationProblem(
            new ValidationProblemDetails(
                ex.Errors.ToDictionary(
                    x => x.Key,
                    x => x.Value))
            {
                Status = 400,
                Title = "One or more validation errors occurred."
            });

        private IActionResult MissingItem() => NotFound(
            new ProblemDetails
            {
                Status = 404,
                Title = "Menu item not found.",
                Detail = "No active menu item matches the supplied ID."
            });
    }
}
