# CoffSip — PostgreSQL Data Model (Week 1, Task 1.1)

Design status: The ERD describes the **intended end-of-Stage-1 schema**, not the schema already implemented in Week 1. Week 1 implements MenuItem CRUD; Week 2 introduces the Identity-backed operator and orders. No payments, real customer data, or files are in scope.

## Entities and fields

### 1. ApplicationUser — authenticated coffee-shop operator (Week 2)
- `Id`: `uuid`, PK. Uses `IdentityUser<Guid>` when implemented.
- `Email`: `varchar(256)`, required. Configure unique email rules with ASP.NET Core Identity.
- `DisplayName`: `varchar(120)`, required.
- ASP.NET Core Identity also manages the hashed password, normalized email/username, security stamps, etc. We do not implement our own password storage.

### 2. MenuItem — product offered by the coffee shop (Week 1)
- `Id`: `uuid`, PK.
- `OwnerId`: `uuid`, FK → `ApplicationUser.Id`; **added and made required in Week 2**.
- `Name`: `varchar(120)`, required.
- `Description`: `varchar(500)`, nullable.
- `Category`: `varchar(60)`, required (e.g., Coffee, Tea, Pastry).
- `Price`: `numeric(12,2)`, required; **strictly > 0**.
- `IsAvailable`: `boolean`, required, default `true`.
- `IsArchived`: `boolean`, required, default `false`; the public DELETE operation will archive an item instead of physically deleting it.
- `CreatedAt`: `timestamptz`, required, UTC.
- `UpdatedAt`: `timestamptz`, required, UTC.

### 3. Order (physical table: CustomerOrder) — one customer order (Week 2)
- `Id`: `uuid`, PK.
- `OwnerId`: `uuid`, required FK → `ApplicationUser.Id`.
- `OrderNumber`: `varchar(24)`, required, unique within an owner.
- `Status`: `varchar(20)`, required; Pending, Preparing, Ready, Completed, or Cancelled.
- `TotalAmount`: `numeric(12,2)`, required; **computed by server** from ordered quantities and price snapshots.
- `CreatedAt`: `timestamptz`, required, UTC.
- `UpdatedAt`: `timestamptz`, required, UTC.

### 4. OrderItem — one distinct menu item on an order (Week 2)
- `Id`: `uuid`, PK.
- `OrderId`: `uuid`, required FK → `CustomerOrder.Id`.
- `MenuItemId`: `uuid`, required FK → `MenuItem.Id`.
- `Quantity`: `integer`, required; >= 1.
- `UnitPrice`: `numeric(12,2)`, required; historical price snapshot, not a live reference to MenuItem.Price.
- Line total = `Quantity * UnitPrice`; the API calculates it without a separate stored field.

## Relationships
- One ApplicationUser → many MenuItems (`OwnerId`).
- One ApplicationUser → many CustomerOrders (`OwnerId`).
- One CustomerOrder → many OrderItems (`OrderId`).
- One MenuItem → many OrderItems (`MenuItemId`).
- Therefore, an Order and MenuItem have a many-to-many relationship **through** OrderItem.

## Business rules and integrity
1. Names and categories are mandatory; prices must be positive; ordered quantities must be >= 1. Configure request validation **and** applicable database constraints.
2. `OrderItem.UnitPrice` is copied from `MenuItem.Price` when the order is recorded, so future menu price changes cannot alter historical totals.
3. `DELETE /api/menu-items/{id}` sets `IsArchived=true`; normal menu queries exclude archived products. Orders retain references to historical archived products.
4. The owning operator of the Order and the referenced MenuItem must match. Validate this server-side before inserting an OrderItem.
5. Do not expose Client-supplied `OwnerId` as an authorization mechanism. In Week 2, obtain ownership from the validated authenticated identity.
6. Keep all timestamps in UTC; use fictional records in demos.
7. Suggested indexes: `MenuItem(OwnerId, IsArchived)`, `CustomerOrder(OwnerId, OrderNumber)` unique, `CustomerOrder(OwnerId, CreatedAt)` and `OrderItem(OrderId)`.

## Incremental migration plan
- Week 1: Implement and migrate `MenuItem` with its nine immediately usable fields (everything above except `OwnerId`). CRUD is a temporary single-shop demo. Document that auth and cross-user isolation have **not** yet been implemented. Do not use it for real customer data or production use.
- Week 2: Add ASP.NET Core Identity with GUID user IDs, create fictional operator seed data, associate existing demo items with the seed user, migrate a **required** `OwnerId` foreign key, and enforce JWT authentication and record ownership. Create `CustomerOrder` and `OrderItem` tables.
- Week 3: Verify foreign keys, edge cases, pagination/indexing, security, automated tests, documentation, and deployment/local-run steps.

## ERD deliverable
Import `docs/database.dbml` into https://dbdiagram.io/ if edits are needed; `docs/erd.png` is a generated overview. The ERD shows the **target schema**, not the Week 1 migration alone.
