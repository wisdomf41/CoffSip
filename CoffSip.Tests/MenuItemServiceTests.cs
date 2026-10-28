using CoffSip.Application.MenuItems;
using CoffSip.Domain.Entities;

namespace CoffSip.Tests
{
    public sealed class MenuItemServiceTests
    {
        [Fact]
        public async Task Create_AssignsOwnerToMenuItem()
        {
            var repository = new FakeMenuItemRepository();
            var service = new MenuItemService(repository);
            var ownerId = Guid.NewGuid();

            var result = await service.CreateAsync(
                new CreateMenuItemRequest(
                    "Latte", "Milk coffee", "Coffee", 4.50m, true),
                ownerId,
                CancellationToken.None);

            var savedItem = Assert.Single(repository.Items);

            Assert.Equal(result.Id, savedItem.Id);
            Assert.Equal<Guid?>(ownerId, savedItem.OwnerId);
            Assert.Equal(1, repository.SaveCount);
        }

        [Fact]
        public async Task Create_RejectsNegativePrice()
        {
            var repository = new FakeMenuItemRepository();
            var service = new MenuItemService(repository);

            var error = await Assert.ThrowsAsync<MenuItemValidationException>(
                () => service.CreateAsync(
                    new CreateMenuItemRequest(
                        "Latte", null, "Coffee", -10m, true),
                    Guid.NewGuid(),
                    CancellationToken.None));

            Assert.Contains("price", error.Errors.Keys);
            Assert.Empty(repository.Items);
        }

        [Fact]
        public async Task Create_RejectsMissingName()
        {
            var repository = new FakeMenuItemRepository();
            var service = new MenuItemService(repository);

            var error = await Assert.ThrowsAsync<MenuItemValidationException>(
                () => service.CreateAsync(
                    new CreateMenuItemRequest(
                        "", null, "Coffee", 4m, true),
                    Guid.NewGuid(),
                    CancellationToken.None));

            Assert.Contains("name", error.Errors.Keys);
        }

        [Fact]
        public async Task Update_RejectsAnotherOperator()
        {
            var ownerId = Guid.NewGuid();
            var item = NewItem(ownerId);
            var repository = new FakeMenuItemRepository();
            repository.Items.Add(item);

            var service = new MenuItemService(repository);

            await Assert.ThrowsAsync<MenuItemOwnershipException>(
                () => service.UpdateAsync(
                    item.Id,
                    new UpdateMenuItemRequest(
                        "Changed", null, "Coffee", 9m, true),
                    Guid.NewGuid(),
                    CancellationToken.None));

            Assert.Equal("Latte", item.Name);
            Assert.Equal(0, repository.SaveCount);
        }

        [Fact]
        public async Task Archive_RejectsAnotherOperator()
        {
            var item = NewItem(Guid.NewGuid());
            var repository = new FakeMenuItemRepository();
            repository.Items.Add(item);

            var service = new MenuItemService(repository);

            await Assert.ThrowsAsync<MenuItemOwnershipException>(
                () => service.ArchiveAsync(
                    item.Id,
                    Guid.NewGuid(),
                    CancellationToken.None));

            Assert.False(item.IsArchived);
            Assert.Equal(0, repository.SaveCount);
        }

        [Fact]
        public async Task Archive_ByOwner_SoftDeletesItem()
        {
            var ownerId = Guid.NewGuid();
            var item = NewItem(ownerId);
            var repository = new FakeMenuItemRepository();
            repository.Items.Add(item);

            var service = new MenuItemService(repository);

            var result = await service.ArchiveAsync(
                item.Id,
                ownerId,
                CancellationToken.None);

            Assert.True(result);
            Assert.True(item.IsArchived);
            Assert.Equal(1, repository.SaveCount);
        }

        [Fact]
        public async Task ListMine_ReturnsOnlyOwnedItems()
        {
            var ownerId = Guid.NewGuid();
            var ownItem = NewItem(ownerId);
            var otherItem = NewItem(Guid.NewGuid());

            var repository = new FakeMenuItemRepository();
            repository.Items.Add(ownItem);
            repository.Items.Add(otherItem);

            var service = new MenuItemService(repository);

            var results = await service.ListMineAsync(
                ownerId,
                CancellationToken.None);

            var result = Assert.Single(results);
            Assert.Equal(ownItem.Id, result.Id);
        }

        private static MenuItem NewItem(Guid ownerId) => new()
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = "Latte",
            Category = "Coffee",
            Price = 4.50m,
            IsAvailable = true
        };

        private sealed class FakeMenuItemRepository : IMenuItemRepository
        {
            public List<MenuItem> Items { get; } = [];

            public int SaveCount { get; private set; }


            // Match the paginated repository contract in automated tests.
            public Task<List<MenuItem>> ListActiveAsync(
                int page,
                int pageSize,
                CancellationToken ct) =>
                Task.FromResult(Items
                    .Where(x => !x.IsArchived)
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());

            public Task<List<MenuItem>> ListOwnedActiveAsync(
                Guid ownerId,
                CancellationToken ct) =>
                Task.FromResult(Items
                    .Where(x => !x.IsArchived && x.OwnerId == ownerId)
                    .ToList());

            public Task<MenuItem?> GetActiveAsync(
                Guid id,
                CancellationToken ct) =>
                Task.FromResult(Items.FirstOrDefault(
                    x => x.Id == id && !x.IsArchived));

            public Task<MenuItem?> GetAnyAsync(
                Guid id,
                CancellationToken ct) =>
                Task.FromResult(Items.FirstOrDefault(
                    x => x.Id == id));

            public Task AddAsync(
                MenuItem item,
                CancellationToken ct)
            {
                Items.Add(item);
                return Task.CompletedTask;
            }

            public Task SaveChangesAsync(CancellationToken ct)
            {
                SaveCount++;
                return Task.CompletedTask;
            }
        }
    }
}
