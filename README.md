# CoffSip

**Coffee Shop Menu Management API**

CoffSip is a C# / ASP.NET Core REST API for managing a coffee shop's menu, including product details, categories, pricing, and availability. It uses PostgreSQL for persistence and a layered solution structure to support future features such as operator accounts and order management.

The current release provides **MenuItem CRUD**, input validation, soft deletion, health checks, and interactive API documentation. It is deployed to Render with a hosted PostgreSQL database.

> **Project status:** Initial public demonstration release for BuildLab Stage 1 — Backend Development. Authentication and authorization are not yet implemented; the hosted API must be used with fictional test data only.

## Live application

| Resource | URL |
| --- | --- |
| Live API | [coffsip.onrender.com](https://coffsip.onrender.com) |
| Swagger UI | [Interactive API documentation](https://coffsip.onrender.com/swagger/index.html) |
| API health | [GET /health](https://coffsip.onrender.com/health) |
| Database readiness | [GET /health/ready](https://coffsip.onrender.com/health/ready) |
| Sample menu items | [GET /api/menu-items](https://coffsip.onrender.com/api/menu-items) |
| Source code | [github.com/wisdomf41/CoffSip](https://github.com/wisdomf41/CoffSip) |

NB: I am using the free tier. So, the Render Free web service may take additional time to respond after a period of inactivity.

## Current features

- Create, list, retrieve, update, and archive coffee-shop menu items.
- Store product name, description, category, price, and availability.
- Validate incoming requests and return appropriate HTTP responses.
- Soft-delete menu items using `IsArchived`, preserving their database records.
- Persist data with Entity Framework Core and PostgreSQL.
- Provide OpenAPI/Swagger documentation and liveness/readiness endpoints.
- Run locally in Visual Studio or deploy using the included Dockerfile.
- Optionally initialize the demonstration database with fictional menu items.

## Technology stack

| Area | Technology |
| --- | --- |
| Language | C# |
| API framework | ASP.NET Core 10 Web API |
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
├── CoffSip.Application/
├── CoffSip.Domain/
├── CoffSip.Infrastructure/
├── docs/
│   └── erd.png
├── api-spec.md
├── data-model.md
├── database.dbml
├── idea-one-pager.md
├── work-plan.md
├── Dockerfile
├── .dockerignore
├── .env.example
├── .gitignore
└── README.md
```

The projects have distinct responsibilities:

- **CoffSip.Api:** HTTP endpoints, application startup, and API configuration.
- **CoffSip.Application:** Menu-item contracts, validation, and application services.
- **CoffSip.Domain:** Core domain entities.
- **CoffSip.Infrastructure:** Entity Framework Core database context, persistence, migrations, and dependency registration.

## API endpoints

| Method | Route | Purpose | Success response |
| --- | --- | --- | --- |
| `GET` | `/health` | Check API liveness | `200 OK` |
| `GET` | `/health/ready` | Check database readiness | `200 OK` when ready |
| `GET` | `/api/menu-items` | List non-archived menu items | `200 OK` |
| `GET` | `/api/menu-items/{id}` | Retrieve an active menu item | `200 OK` |
| `POST` | `/api/menu-items` | Create a menu item | `201 Created` |
| `PUT` | `/api/menu-items/{id}` | Update an active menu item | `200 OK` |
| `DELETE` | `/api/menu-items/{id}` | Archive a menu item | `204 No Content` |

Invalid input, including non-positive prices, returns `400 Bad Request`. Missing or archived menu items return `404 Not Found`. Deleting a menu item marks it as archived rather than removing its database record.

### Example request

`POST /api/menu-items`

```json
{
  "name": "Graceme",
  "description": "Coffee with steamed milk and vanilla syrup",
  "category": "Coffee",
  "price": 4.50,
  "isAvailable": true
}
```

Use Swagger UI to inspect request and response schemas. **Only send test data to this public demonstration API.**

## Run locally

### Prerequisites

- .NET 10 SDK.
- PostgreSQL server (version 17 used during development).
- Visual Studio with the ASP.NET and web development workload.

### Setup

1. Clone the repository and open `CoffSip.slnx` in Visual Studio.
2. Create a local PostgreSQL database named `coffsip_db` if it does not already exist.
3. In Solution Explorer, right-click **CoffSip.Api → Manage User Secrets**. Add the following configuration, replacing the password placeholder with your own local PostgreSQL password:

   ```json
   {
     "ConnectionStrings": {
       "CoffSipDatabase": "Host=localhost;Port=5432;Database=coffsip_db;Username=postgres;Password=REPLACE_WITH_LOCAL_PASSWORD"
     },
     "InitializeDemoDatabase": true
   }
   ```

4. Rebuild the solution. To apply the migration manually, open **Tools → NuGet Package Manager → Package Manager Console** and run:

   ```powershell
   Update-Database -Project CoffSip.Infrastructure -StartupProject CoffSip.Api -Context AppDbContext
   ```

5. Set **CoffSip.Api** as the startup project and launch its **https** profile.
6. Open Swagger UI at the HTTPS address shown by Visual Studio, followed by `/swagger` (for example, `https://localhost:5001/swagger`).

You can also check local database readiness at `/health/ready`. Do not commit real passwords, database credentials, `.env` files, or `secrets.json`.

## Render deployment

CoffSip is deployed as a **Docker Web Service** connected to a separate **Render PostgreSQL 17** instance in the same region.

The repository root contains the `Dockerfile`, so the Render service uses the repository root as its build context. ASP.NET Core listens on the port supplied by Render.

The deployment uses these Render environment variables:

| Key | Purpose |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` on Render. |
| `ConnectionStrings__CoffSipDatabase` | Private PostgreSQL connection string; supplied in Render settings, never source control. |
| `InitializeDemoDatabase` | `true` for this demonstration deployment, enabling database initialization and fictional seed data. |

A PostgreSQL connection string typically follows this pattern:

```text
Host=YOUR_INTERNAL_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USER;Password=YOUR_PASSWORD
```

Use the **internal host** provided by Render when the API and database are in the same compatible region. Keep actual credentials in Render's environment settings.

The current demonstration startup can initialize the database automatically. For a long-term production environment, database migrations and seed-data operations should be moved to a controlled deployment process rather than performed on ordinary application startup.

### Deployment checks

The publicly deployed endpoints have been verified to return a ready database status and sample menu items:

- [`/health/ready`](https://coffsip.onrender.com/health/ready) — responds with `{ "status": "ready" }` when PostgreSQL is reachable.
- [`/api/menu-items`](https://coffsip.onrender.com/api/menu-items) — returns fictional menu products.
- [`/swagger/index.html`](https://coffsip.onrender.com/swagger/index.html) — documents the available API operations.

## Documentation

- [API specification](api-spec.md) — routes, requests, responses, and status codes.
- [Data model](data-model.md) — domain design and planned entity relationships.
- [Database model (DBML)](database.dbml) — schema design source.
- [Entity-relationship diagram](docs/erd.png) — visual schema overview.
- [Project overview](idea-one-pager.md) — problem statement and scope.
- [Work plan](work-plan.md) — project tasks, risks, and milestones.

The ERD and planning documents show the intended broader system design. Not all proposed entities are implemented in the current release.

## Scope and next steps

**Implemented:** MenuItem operations, input validation, archival, PostgreSQL persistence, local setup, Docker deployment, and public Swagger documentation. The CRUD behavior was manually tested through Swagger, including successful and invalid requests.

**Planned:** Authentication and role-based authorization, menu ownership controls, order and order-item workflows, automated API tests, and further security and operational hardening.

> **Security notice:** The current demo exposes menu-item write endpoints without authentication. It is intended for an assessment environment using fictional data, **not** for real customers, payments, or sensitive business records. Restrict access and complete authorization and security testing before any production rollout.

## Version and project milestone

The initial source release was committed as `Add menu management API`. The assessment milestone is tagged [`Week_1`](https://github.com/wisdomf41/CoffSip/tree/Step_1).

This is an early functional release, not a claim that the application is production-ready.

AI assistance was used when needed for more architectural planning, technical explanations and documentation.
