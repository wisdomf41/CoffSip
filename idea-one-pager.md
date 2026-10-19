 # CoffSip — Coffee Shop Menu & Order Management API

Track: Backend Development

Stack: C#, ASP.NET Core 10 Web API, PostgreSQL, Entity Framework Core, Docker

Target user: Small independent coffee-shop owners and operators.

Problem: Paper-based or disconnected processes make it difficult for a coffee-shop operator to keep menu availability, prices, and order statuses accurate. This can cause delays and order mistakes.

Solution: A REST API for recording coffee-shop menu items and customer orders in one database, with operator authentication and ownership controls.

Entities: ApplicationUser, MenuItem, Order, OrderItem.

Core flow: Add a menu item → list available items → create an order → record its items and total → update order status → review order history.

In scope: Menu CRUD, secure sign-up/login, owner-scoped orders and order items, validation and consistent error responses, automated tests and API documentation.

later: Payments, React/mobile UI, real-time chat/notifications, product image uploads, delivery and loyalty features.

Top three risks: Scope creep; ownership and authorization defects; database/deployment setup difficulties.

Data policy: Synthetic test data only; no real customer passwords or personal details.
