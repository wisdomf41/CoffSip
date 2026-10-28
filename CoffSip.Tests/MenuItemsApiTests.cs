using CoffSip.Application.MenuItems;
using CoffSip.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace CoffSip.Tests
{
    public sealed class MenuItemsApiTests
    {
        [Fact]
        public async Task Get_InvalidId_Returns400()
        {
            using var factory = new ApiFactory();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync(
                "/api/menu-items/invalid-id");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutJwt_Returns401()
        {
            using var factory = new ApiFactory();
            using var client = factory.CreateClient();

            using var response = await client.PostAsJsonAsync(
                "/api/menu-items",
                new
                {
                    name = "Latte",
                    description = "Coffee with milk",
                    category = "Coffee",
                    price = 4.50m,
                    isAvailable = true
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Update_ByAnotherOperator_Returns403()
        {
            using var factory = new ApiFactory();
            using var client = factory.CreateClient();

            var anotherOperatorId = Guid.NewGuid();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    CreateTestToken(anotherOperatorId));

            using var response = await client.PutAsJsonAsync(
                $"/api/menu-items/{factory.ExistingItemId}",
                new
                {
                    name = "Updated Latte",
                    description = "Updated description",
                    category = "Coffee",
                    price = 6.00m,
                    isAvailable = true
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Get_NonexistentItem_Returns404()
        {
            using var factory = new ApiFactory();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync(
                $"/api/menu-items/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private const string TestKey =
            "CoffSipTestSigningKeyOnly_ForAutomatedIntegrationTests_2026_12345";

        private static string CreateTestToken(Guid userId)
        {
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(TestKey)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "CoffSip.Tests",
                audience: "CoffSip.Tests",
                claims:
                [
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
                ],
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private sealed class ApiFactory : WebApplicationFactory<Program>
        {
            private readonly FakeMenuItemRepository _repository = new();

            public Guid ExistingItemId { get; } = Guid.NewGuid();

            public ApiFactory()
            {
                _repository.Items.Add(new MenuItem
                {
                    Id = ExistingItemId,
                    OwnerId = Guid.NewGuid(),
                    Name = "Latte",
                    Category = "Coffee",
                    Price = 4.50m,
                    IsAvailable = true
                });
            }

            protected override IHost CreateHost(IHostBuilder builder)
            {
                builder.ConfigureHostConfiguration(config =>
                    config.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["Jwt:Key"] = TestKey,
                            ["Jwt:Issuer"] = "CoffSip.Tests",
                            ["Jwt:Audience"] = "CoffSip.Tests",
                            ["ConnectionStrings:CoffSipDatabase"] =
                                "Host=localhost;Port=1;Database=unused_test_db;Username=test;Password=test",
                            ["InitializeDemoDatabase"] = "false"
                        }));

                return base.CreateHost(builder);
            }

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IMenuItemRepository>();

                    services.AddSingleton<IMenuItemRepository>(
                        _repository);
                });
            }
        }

        private sealed class FakeMenuItemRepository : IMenuItemRepository
        {
            public List<MenuItem> Items { get; } = [];


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

            public Task SaveChangesAsync(CancellationToken ct) =>
                Task.CompletedTask;
        }
    }
}
