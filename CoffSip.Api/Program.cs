
using CoffSip.Application.MenuItems;
using CoffSip.Infrastructure;
using CoffSip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<MenuItemService>();
builder.Services.AddInfrastructure(builder.Configuration);

var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(port) &&
    int.TryParse(port, out var portNumber) &&
    portNumber is > 0 and <= 65535)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
}

var app = builder.Build();

if (app.Configuration.GetValue<bool>("InitializeDemoDatabase"))
{
    await DatabaseInitializer.InitializeAsync(app.Services);
}

// Enable Swagger UI using the built-in OpenAPI document.
app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/openapi/v1.json",
        "CoffSip API v1");

    options.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "ok",
        service = "CoffSip.Api"
    }));

app.MapGet("/health/ready",
    async (AppDbContext db, CancellationToken ct) =>
        await db.Database.CanConnectAsync(ct)
            ? Results.Ok(new { status = "ready" })
            : Results.StatusCode(
                StatusCodes.Status503ServiceUnavailable));

app.Run();
