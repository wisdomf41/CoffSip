# CoffSip — BuildLab Stage 1 Work Plan

## my first week plan

Product: CoffSip — Coffee Shop Menu & Order Management API
Stack: C#, ASP.NET Core 10 Web API, PostgreSQL, Entity Framework Core, ASP.NET Core Identity/JWT, xUnit, Docker
Audience: Small independent coffee-shop owners and operators
Scope: Backend REST API only; use fictional records throughout.

## Objective
Provide a dependable backend for maintaining a coffee-shop menu and recording simple customer orders. One owner/operator can maintain menu availability, create orders, update statuses, and review order details without paper-based tracking.

## Week 1 — Design and core CRUD
- Submit the idea one-pager and this one-page work plan.
- Create a public GitHub repository with a starter README and `.env.example` (no secrets).
- Document four planned entities and their relationships in an ERD.
- Write the REST API specification **before implementing endpoints**.
- Set up the ASP.NET Core solution, EF Core, and PostgreSQL migrations.
- Implement `MenuItem` create, list, read-by-ID, update, and archive (DELETE) operations.
- Validate inputs and return correct HTTP status codes; seed fictional sample items.
- Supply example HTTP requests; demonstrate CRUD to mentors in five minutes.
- Record feedback, commit work incrementally, and tag the assessed version `week-1`.

## Week 2 — Authentication and robustness
- Add registration/login using ASP.NET Core Identity and JWT.
- Associate menu items and orders with their owning authenticated operator.
- Apply owner-only access checks and appropriate 401/403/404 responses.
- Implement `Order` and `OrderItem` relationships, order totals, and status changes.
- Ensure order prices use a snapshot of the menu price at order creation.
- Introduce consistent validation errors and central exception handling.
- Resolve duplicate/invalid-ID/deletion edge cases; keep an issue log with at least five entries.
- Address mentor feedback and tag the assessed version `week-2`.

## Week 3 — Testing, documentation, and delivery
- Add xUnit unit and integration tests with an isolated test database.
- Test happy paths plus validation and authentication/authorization failures.
- Complete OpenAPI documentation and sample `.http` requests.
- Review configuration security, secret handling, CORS, auth rate limiting, pagination, and indexes.
- Verify Docker Compose local setup and document a clean-machine run procedure; deployment is optional under the guide if the run guide works.
- Complete README/case study, security/performance notes, technical reflection, required AI-and-sources declaration, and five-minute video.
- Tag `week-3`, check public access, and submit through the official form.

## Top three risks and mitigations
1. Scope growth: Payments, frontend, GraphQL, Redis, Kubernetes, and real-time features are excluded from Stage 1. Implement only the guide's evidence-based requirements.
2. Authentication and data ownership: Establish ownership rules and verify cross-user access with integration tests before Week 2 submission.
3. Environment and delivery failures: Use pinned package versions, reproducible migrations, `.env.example`, Docker Compose, and a documented clean-run verification.

## Key design decisions
- Use PostgreSQL because menu items, orders, and order items have relational integrity requirements.
- Use UUID primary keys and UTC timestamps; prices use `numeric(12,2)`, never floating-point.
- Store an order item's unit price snapshot so historical order totals do not change when menu prices change.
- `DELETE /api/menu-items/{id}` archives items instead of destroying historical order references.
- Week 1 implementation is a single-shop demo catalog; Week 2 introduces ASP.NET Core Identity users and ownership migration. The accompanying ERD describes the end-of-project model.
- The deployment uses one configured currency for all menu prices and orders; currency conversion and payments are out of scope.

## Submission evidence
Weekly evidence includes repository link, an accessible local run guide or live API, tagged release, sample requests, and an update note. Week 1 also includes this plan, idea one-pager, ERD, and API specification. The official guide requires material AI use to be disclosed in the applicable submission materials.
