# CoffSip — Coffee Shop Menu Management API

**BuildLab Stage 1 | Backend Development | Week 1**

CoffSip is a REST API for a small coffee-shop operator to manage menu items, prices, categories, and availability. The Week 1 deliverable focuses on MenuItem CRUD; authentication, order management, and ownership checks are planned for Week 2.

## Technology stack

- C# and ASP.NET Core 10 Web API
- PostgreSQL 17 and Entity Framework Core 10
- OpenAPI and Swagger UI for interactive testing
- Layered solution: `CoffSip.Api`, `CoffSip.Application`, `CoffSip.Domain`, `CoffSip.Infrastructure`
- Dockerfile-based deployment target: Render

## Repository structure


CoffSip/
├── CoffSip.slnx
├── CoffSip.Api/
├── CoffSip.Application/
├── CoffSip.Domain/
├── CoffSip.Infrastructure/
├── docs/
│   ├── idea-one-pager.md
│   ├── work-plan.md
│   ├── data-model.md
│   ├── database.dbml
│   ├── erd.png
│   └── api-spec.md
├── Dockerfile
├── .dockerignore
├── .gitignore
└── .env.example

## Local setup

**Prerequisites:** .NET 10 SDK, a running PostgreSQL server, and Visual Studio with ASP.NET and web development tools.

1. Open `CoffSip.slnx` in Visual Studio.
2. Create a local PostgreSQL database named `coffsip_db` if it does not already exist.
3. Right-click the `CoffSip.Api` project and select **Manage User Secrets**. Add a local connection string (replace the placeholder password):

   ```json
   {
     "ConnectionStrings": {
       "CoffSipDatabase": "Host=localhost;Port=5432;Database=coffsip_db;Username=postgres;Password=REPLACE_WITH_LOCAL_PASSWORD"
     },
     "InitializeDemoDatabase": true
   }
   ```

4. Rebuild the solution. If the schema has not yet been applied, open **Tools → NuGet Package Manager → Package Manager Console** and run:

   ```powershell
   Update-Database -Project CoffSip.Infrastructure -StartupProject CoffSip.Api -Context AppDbContext
   ```

5. Set `CoffSip.Api` as the startup project and run the **https** profile.
6. Open Swagger UI at `https://localhost:<your-https-port>/swagger` (for example, `https://localhost:5001/swagger` when using that port).

Do not commit real connection strings, passwords, or `secrets.json`.

## MenuItem API (Week 1)

| Method | Route | Purpose | Typical success response |
|---|---|---|---|
| GET | `/health` | API liveness | `200 OK` |
| GET | `/health/ready` | Database readiness | `200 OK` |
| GET | `/api/menu-items` | List non-archived menu items | `200 OK` |
| GET | `/api/menu-items/{id}` | Get one active menu item | `200 OK` |
| POST | `/api/menu-items` | Create a menu item | `201 Created` |
| PUT | `/api/menu-items/{id}` | Update a menu item | `200 OK` |
| DELETE | `/api/menu-items/{id}` | Archive a menu item | `204 No Content` |

Example POST body:

```json
{
  "name": "mycoffee",
  "description": "sweet coffee with steamed milk",
  "category": "Coffee",
  "price": 4.50,
  "isAvailable": true
}
```

Invalid price input is rejected with `400 Bad Request`, and missing or archived menu items return `404 Not Found`. Deletion archives a product rather than physically removing it.

See [`docs/api-spec.md`](docs/api-spec.md) for request and response details, and [`docs/data-model.md`](docs/data-model.md) for the proposed database schema and design choices.

## Current scope and limitations

- Week 1 supports menu-item management only.
- Authentication, authorization, user ownership, and order entities are planned for Week 2; **do not use this version with sensitive or real customer data**.
- Use fictional products and test data. The demo is intended for assessment, not for production use.
- Database setup and migrations are required for a new environment.

## Deployment and submission

Target hosting: **Render Web Service (Docker)** with a separate hosted PostgreSQL database. Configure the cloud database connection using the Render environment setting `ConnectionStrings__CoffSipDatabase`; never hardcode the cloud password. The cloud deployment URL will be added here **after successful deployment and verification**.

Week 1 deliverables include the public repository, ERD, API specification, executable API or local run guide, sample requests (Swagger UI), `week-1` Git tag, a weekly update note, and a five-minute mentor demonstration.
