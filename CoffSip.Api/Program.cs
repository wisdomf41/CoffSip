
using CoffSip.Api.Authentication;
using CoffSip.Application.MenuItems;
using CoffSip.Infrastructure;
using CoffSip.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


builder.Services.AddOpenApi(options =>
{
    // Keep Render HTTPS compatibility and enable Swagger JWT authorization for protected routes.
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers =
        [
            new OpenApiServer { Url = "/" }
        ];

        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT access token."
            };

        foreach (var path in document.Paths)
        {
            if (path.Value?.Operations is not { } operations)
                continue;

            foreach (var method in path.Value.Operations)
            {
                var route = path.Key;
                var verb = method.Key.ToString();

                var requiresAuth =
                    (route.Equals("/api/menu-items/mine",
                        StringComparison.OrdinalIgnoreCase) &&
                     verb.Equals("Get",
                        StringComparison.OrdinalIgnoreCase)) ||

                    (route.Equals("/api/menu-items",
                        StringComparison.OrdinalIgnoreCase) &&
                     verb.Equals("Post",
                        StringComparison.OrdinalIgnoreCase)) ||

                    (route.Equals("/api/menu-items/{id}",
                        StringComparison.OrdinalIgnoreCase) &&
                     (verb.Equals("Put",
                         StringComparison.OrdinalIgnoreCase) ||
                      verb.Equals("Delete",
                         StringComparison.OrdinalIgnoreCase)));

                if (!requiresAuth)
                    continue;

                method.Value.Security ??= [];

                method.Value.Security.Add(
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(
                            "Bearer", document)] = []
                    });
            }
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddScoped<MenuItemService>();
builder.Services.AddInfrastructure(builder.Configuration);


// Configure JWT authentication and validate token signatures, issuer, audience and expiration.
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must contain at least 32 bytes.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

builder.Services.AddScoped<JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            RequireExpirationTime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            NameClaimType = "sub",
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Limit authentication requests per client to reduce automated login and registration abuse.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});



var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(port) &&
    int.TryParse(port, out var portNumber) &&
    portNumber is > 0 and <= 65535)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
}

var app = builder.Build();

// Handle unexpected production errors without exposing internal stack traces.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

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

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
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

// Expose the application entry point to integration tests.
public partial class Program { }