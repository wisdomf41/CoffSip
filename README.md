# CoffSip

**Coffee Shop Menu Management API**

CoffSip is a C# / ASP.NET Core REST API for managing coffee-shop menu items, including product details, categories, prices, and availability. It uses PostgreSQL for persistence and a four-project layered architecture.

Developed for **BuildLab Stage 1 — Backend Development**, CoffSip progressed from basic menu management in Week 1 to **user registration, JWT login, authenticated menu operations, and operator ownership controls in Week 2**.

> **Project status:** **Week 2 completed, merged into `main`, deployed to Render, and verified both locally and on the live API.** Identity registration, JWT login, authenticated menu creation, owned-item listing, and cross-operator access protection have been tested. The `week-2` Git tag is published. Use fictional data only; CoffSip is not yet a production-ready customer application.

## Live application

| Resource | URL |
| --- | --- |
| Live API | [coffsip.onrender.com](https://coffsip.onrender.com) |
| Swagger UI | [Interactive API documentation](https://coffsip.onrender.com/swagger/index.html) |
| API health | [GET /health](https://coffsip.onrender.com/health) |
| Database readiness | [GET /health/ready](https://coffsip.onrender.com/health/ready) |
| Sample menu items | [GET /api/menu-items](https://coffsip.onrender.com/api/menu-items) |
| Source code | [github.com/wisdomf41/CoffSip](https://github.com/wisdomf41/CoffSip) |

**Hosting note:** Render's Free web service may take additional time to respond after a period of inactivity.

## Features

### Week 1 — Menu management

- Create, list, retrieve, update, and archive menu items.
- Store product names, descriptions, categories, prices, and availability.
- Validate incoming requests and return appropriate HTTP status codes.
- Archive items through `IsArchived` rather than permanently deleting them.
- Persist data using Entity Framework Core and PostgreSQL migrations.
- Expose OpenAPI/Swagger documentation and liveness/readiness endpoints.
- Deploy using the root `Dockerfile` and initialize fictional demonstration data when configured.

### Week 2 — Authentication and ownership

- Register operator accounts using **ASP.NET Core Identity**, including secure password hashing and duplicate-account validation.
- Verify login credentials and issue **30-minute JWT Bearer access tokens**.
- Validate token signatures, issuer, audience, and expiration.
- Keep public menu-reading endpoints available while requiring authentication for menu-item changes.
- Assign `OwnerId` from the authenticated user's JWT identity when creating an item.
- Allow operators to list their own items through `GET /api/menu-items/mine`.
- Restrict updates and archival to the owning operator; return `403 Forbidden` for another operator's item.
- Return `401 Unauthorized` for unauthenticated protected requests.
- Support JWT authentication through Swagger's **Authorize** button.
- Retain earlier fictional items with `OwnerId = NULL` so the ownership migration preserves existing data. Ordinary operators cannot modify these unowned legacy items.

## Technology stack

| Area | Technology |
| --- | --- |
| Language | C# |
| API framework | ASP.NET Core 10 Web API |
| Authentication | ASP.NET Core Identity and JWT Bearer |
| Data access | Entity Framework Core 10 |
| Database | PostgreSQL 17 |
| API documentation | OpenAPI and Swagger UI |
| Architecture | API, Application, Domain, Infrastructure layers |
| Containerization | Docker |
| Hosting | Render Web Service and Render PostgreSQL |
| Development environment | Visual Studio |

## Repository structure

```text
CoffSip/
├── CoffSip.slnx
├── CoffSip.Api/
│   ├── Authentication/
│   └── Controllers/
├── CoffSip.Application/
│   ├── Authentication/
│   └── MenuItems/
├── CoffSip.Domain/
│   └── Entities/
├── CoffSip.Infrastructure/
│   ├── Data/
│   └── Identity/
├── docs/
│   └── erd.png
├── api-spec.md
├── data-model.md
├── database.dbml
├── idea-one-pager.md
├── work-plan.md
├── progress-report.md
├── Dockerfile
├── .dockerignore
├── .env.example
├── .gitignore
└── README.md
```

The four projects have distinct responsibilities:

- **CoffSip.Api:** HTTP controllers, JWT validation, token issuance, Swagger configuration, and application startup.
- **CoffSip.Application:** Authentication and menu-item contracts, validation, business logic, and ownership rules.
- **CoffSip.Domain:** Core menu-item entity.
- **CoffSip.Infrastructure:** PostgreSQL persistence, migrations, ASP.NET Core Identity users, and dependency registration.

## API endpoints

| Method | Route | Access | Purpose | Typical success |
| --- | --- | --- | --- | --- |
| `GET` | `/health` | Public | API liveness | `200 OK` |
| `GET` | `/health/ready` | Public | Database readiness | `200 OK` |
| `POST` | `/api/auth/register` | Public | Register an operator | `201 Created` |
| `POST` | `/api/auth/login` | Public | Verify credentials and issue JWT | `200 OK` |
| `GET` | `/api/menu-items` | Public | List non-archived menu items | `200 OK` |
| `GET` | `/api/menu-items/{id}` | Public | Retrieve an active menu item | `200 OK` |
| `GET` | `/api/menu-items/mine` | JWT required | List the signed-in operator's items | `200 OK` |
| `POST` | `/api/menu-items` | JWT required | Create an owned menu item | `201 Created` |
| `PUT` | `/api/menu-items/{id}` | Owner JWT required | Update an owned item | `200 OK` |
| `DELETE` | `/api/menu-items/{id}` | Owner JWT required | Archive an owned item | `204 No Content` |

**Error responses:** Invalid input and duplicate registration return `400 Bad Request`. Missing or archived items return `404 Not Found`; unauthenticated protected calls return `401 Unauthorized`; authenticated operators attempting to modify another operator's items receive `403 Forbidden`.

### Example: register an operator

`POST /api/auth/register`

```json
{
  "email": "operator.a@coffsip.example",
  "password": "CoffeeOperatorA!2026"
}
```

On success, the API returns `201 Created` with a generated user ID. Use unique fictional accounts for demonstrations.

### Example: log in

`POST /api/auth/login`

```json
{
  "email": "operator.a@coffsip.example",
  "password": "CoffeeOperatorA!2026"
}
```

A successful response includes `accessToken`, `tokenType` (`Bearer`), and `expiresAtUtc`. JWT access tokens expire after approximately 30 minutes.

### Example: create a menu item

`POST /api/menu-items` — **JWT required**

```json
{
  "name": "Caramel Latte",
  "description": "Espresso with milk and caramel",
  "category": "Coffee",
  "price": 5.50,
  "isAvailable": true
}
```

**Do not send `OwnerId` in this request.** The API derives ownership from the authenticated JWT subject (`sub`) claim.

### Testing protected endpoints in Swagger

1. Open [Swagger UI](https://coffsip.onrender.com/swagger/index.html) (or the local instance) and execute `POST /api/auth/login` with a fictional registered account.
2. Copy the returned `accessToken` (do not share or log it).
3. Click **Authorize** and paste the token **without** the `Bearer` prefix; Swagger applies it automatically.
4. Execute protected endpoints such as `POST /api/menu-items` or `GET /api/menu-items/mine`.
5. To test ownership, log in as another operator, replace the token, and try updating the first operator's item. The API should return `403 Forbidden`.

## Run locally

### Prerequisites

- .NET 10 SDK.
- PostgreSQL server (version 17 used during development).
- Visual Studio with the ASP.NET and web development workload.

### Setup

1. Clone the repository and open `CoffSip.slnx` in Visual Studio.
2. Create a local PostgreSQL database named `coffsip_db` if needed.
3. In Solution Explorer, right-click **CoffSip.Api → Manage User Secrets**. Configure the local database connection and JWT settings. This example uses placeholders only:

   ```json
   {
     "ConnectionStrings": {
       "CoffSipDatabase": "Host=localhost;Port=5432;Database=coffsip_db;Username=postgres;Password=REPLACE_WITH_LOCAL_PASSWORD"
     },
     "InitializeDemoDatabase": true,
     "Jwt": {
       "Issuer": "CoffSip",
       "Audience": "CoffSip.Api",
       "Key": "REPLACE_WITH_A_STRONG_RANDOM_SECRET_AT_LEAST_32_BYTES"
     }
   }
   ```

   Generate a cryptographically random JWT signing key rather than using the placeholder. Keep the full secret in User Secrets, not source control.

4. Rebuild the solution. To apply existing migrations against the **local** database, open **Tools → NuGet Package Manager → Package Manager Console** and run:

   ```powershell
   Update-Database -Project CoffSip.Infrastructure -StartupProject CoffSip.Api -Context AppDbContext
   ```

   This includes the initial menu, Identity tables, and nullable menu ownership migrations.

5. Set `CoffSip.Api` as the startup project and run its **https** profile.
6. Open [local Swagger](https://localhost:5001/swagger/index.html) (or the HTTPS port shown by Visual Studio).
7. Test registration, login, and JWT-protected menu operations with fictional data.

Never commit real passwords, JWT signing keys, database URLs with credentials, `.env`, or `secrets.json`.

## Render deployment

CoffSip uses a **Render Docker Web Service** connected to a separate **Render PostgreSQL 17** database. The repository root contains the Dockerfile, and the API accepts the port supplied by Render.

| Environment variable | Purpose |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Set to `Production`. |
| `ConnectionStrings__CoffSipDatabase` | Private PostgreSQL connection string. |
| `InitializeDemoDatabase` | Set to `true` for this fictional demonstration environment when using startup initialization/migrations. |
| `Jwt__Issuer` | `CoffSip` |
| `Jwt__Audience` | `CoffSip.Api` |
| `Jwt__Key` | Strong, randomly generated production-deployment signing key; keep private. |

A connection-string placeholder looks like this:

```text
Host=YOUR_INTERNAL_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USER;Password=YOUR_PASSWORD
```

Use Render's internal database host when both services are in compatible regions. Do not expose the completed connection string or signing key.

**Week 2 deployment completed:** The authentication feature was reviewed and merged through [Pull Request #2](https://github.com/wisdomf41/CoffSip/pull/2) into `main`. Render is serving the updated API, including the Identity/ownership changes. JWT settings are provided through Render environment variables rather than committed secrets; local User Secrets do **not** transfer to Render. The verified Week 2 release is marked by the [`week-2` tag](https://github.com/wisdomf41/CoffSip/tree/week-2).

For a long-term production system, migrations and fictional data seeding should be moved to a controlled deployment process rather than performed automatically on ordinary startup.

### Deployment checks

Verified public demonstration endpoints:

- [`/health/ready`](https://coffsip.onrender.com/health/ready) — ready status when the hosted PostgreSQL database is reachable.
- [`/api/menu-items`](https://coffsip.onrender.com/api/menu-items) — fictional menu items.
- [`/swagger/index.html`](https://coffsip.onrender.com/swagger/index.html) — interactive API documentation.

**Week 2 live verification completed:** A fictional operator account was registered and used to log in and obtain a JWT; Swagger's **Authorize** button enabled protected menu creation (`201 Created`). The new product appeared in the operator's `/api/menu-items/mine` results (`200 OK`). When a second operator attempted to update that product, the API correctly returned `403 Forbidden`. An unauthenticated write request returned `401 Unauthorized`, and `/health/ready` returned `200 OK`.

## Verification and testing

| Scenario | Expected status | Result |
| --- | --- | --- |
| Register a new fictional operator | `201` | Passed |
| Register a duplicate email | `400` | Passed |
| Log in with valid credentials | `200` | Passed |
| Log in with invalid credentials | `401` | Passed |
| List the public menu | `200` | Passed |
| Access a protected endpoint without a JWT | `401` | Passed |
| Create an item with a valid JWT | `201` | Passed |
| List owned items | `200` | Passed |
| Update an item owned by another user | `403` | Passed |
| Archive an item owned by another user | `403` | Passed |
| Update or archive one's own item | `200` / `204` | Passed |
| Invalid input, missing/archived item | `400` / `404` | Passed |
| Database readiness | `200` | Passed |

The table above summarizes manual tests performed through **local Swagger**. The following additional tests were verified against the deployed Render service:

| Live scenario | HTTP result | Outcome |
| --- | --- | --- |
| Database readiness | `200` | Passed |
| Unauthenticated menu creation | `401` | Passed |
| Operator registration and JWT login | Successful | Passed |
| Authenticated menu creation | `201` | Passed |
| Operator's own menu listing (`/mine`) | `200` | Passed |
| Second operator attempts to update another operator's item | `403` | Passed |

These checks cover the core Week 2 release; they are not a comprehensive automated security test suite. Automated testing is planned for Week 3.

## Documentation

- [API specification](api-spec.md) — the original Week 1 API design; the implemented Week 2 authentication and ownership endpoints are documented in this README.
- [Data model](data-model.md) — domain model and initial design choices.
- [Database model (DBML)](database.dbml) — original schema design source.
- [Entity-relationship diagram](docs/erd.png) — original design overview.
- [Project overview](idea-one-pager.md) — problem statement and scope.
- [Work plan](work-plan.md) — tasks, risks, and milestones.
- [Initial progress report](progress-report.md) — development and initial deployment summary.

The original ERD, DBML, and API specification describe the initial design and may include proposed entities that have not been implemented. The implemented Week 2 relationship is `AspNetUsers.Id` → `MenuItems.OwnerId` (one operator to many menu items); `OwnerId` is nullable for legacy demo records.

## Scope and next steps

**Implemented and verified through Week 2 (local and Render):** MenuItem CRUD, soft deletion, PostgreSQL persistence, Identity registration, JWT login, authenticated write endpoints, owner-only update/archive, owned-item listing, input validation, Swagger Bearer authorization, and health checks.

**Not implemented:** Full order/order-item workflows, payments, web frontend, email verification, password-reset and refresh-token flows, comprehensive automated tests, and advanced security/performance controls. These are outside the scope of the current demonstration milestone.

> **Security notice:** Use fictional data only. While authentication and ownership improve access control, this API has not undergone comprehensive production security testing and is not suitable for real customer data or payment processing. Additional safeguards such as rate limiting, monitoring, and a controlled deployment migration process are still needed.

## Version and milestones

- **[Week 1 (`week-1`)](https://github.com/wisdomf41/CoffSip/tree/week-1):** Initial menu CRUD implementation, PostgreSQL integration, and public Docker deployment.
- **[Week 2 (`week-2`)](https://github.com/wisdomf41/CoffSip/tree/week-2):** Identity registration, JWT login, authenticated menu operations, ownership enforcement, Swagger Bearer authorization, and verified Render deployment. This milestone points to merge commit `4435f6e`.
- **Week 3 (planned):** Automated tests, documentation improvements, security/performance hardening, and final delivery.

## Development acknowledgements
AI assistance was used also when needed for more architectural planning, technical explanations and documentation.

## Testing

CoffSip uses xUnit for automated testing.

The test suite includes:
- Menu validation tests
- Menu ownership and authorization tests
- Soft-delete behavior tests
- API integration tests for HTTP 400, 401, 403, and 404 responses

Run all tests:

```bash
dotnet test CoffSip.Tests/CoffSip.Tests.csproj
```

The API integration tests use an isolated test host and a
fake menu repository. They do not test actual PostgreSQL persistence.

## Security

- ASP.NET Core Identity handles password hashing and account management.
- JWT authentication protects menu management endpoints.
- Operators can modify and archive only their own menu items.
- Authentication endpoints use rate limiting.
- Secrets are managed through configuration and environment variables.

## Pagination

The public menu endpoint supports pagination:

GET /api/menu-items?page=1&pageSize=10

- Default page: 1
- Default page size: 10
- Maximum page size: 50
- Invalid pagination parameters return HTTP 400.

A PostgreSQL index supports active-menu filtering and pagination ordering.

## Current Limitations

- No customer ordering or payment functionality.
- Authentication rate limiting is basic and requires trusted
  proxy configuration review before larger-scale production use.
- Integration tests use fake persistence rather than a real test database.