# CoffSip — Backend Development Progress Report

**Project:** CoffSip — Coffee Shop Menu Management API  
**Programme:** BuildLab Stage 1 — Backend Development  
**Milestone:** Initial API Development and Deployment  
**Status:** Completed

## Work Completed

- Designed a layered ASP.NET Core 10 Web API solution comprising API, Application, Domain, and Infrastructure projects.
- Designed the PostgreSQL database schema and Entity Framework Core data model.
- Implemented MenuItem CRUD endpoints for creating, retrieving, updating, and archiving coffee-shop products.
- Added request validation and appropriate HTTP responses, including 200, 201, 204, 400, and 404.
- Tested the CRUD operations locally using Swagger UI.
- Configured PostgreSQL 17 and Entity Framework Core migrations.
- Published the project to GitHub with supporting documentation, an ERD, and an API specification.
- Deployed the API to Render using Docker.
- Connected the deployed application to a hosted PostgreSQL database.
- Verified the public database-readiness endpoint and menu listing.
- Published the initial milestone tag `Step_1`.

## Challenges and Resolutions

Local database connection issues were resolved by configuring the connection string securely through ASP.NET Core User Secrets. Deployment configuration was completed using Render environment variables and a Docker-based build.

## Current Limitations

Authentication, authorization, order processing, and user ownership controls have not yet been implemented. The current release is intended for demonstration using fictional data rather than real customer information.

## Next Development Milestone

The next phase will focus on authentication, role-based authorization, ownership checks, and order-management functionality.

## Project Links

**GitHub:** https://github.com/wisdomf41/CoffSip

**Live API:** https://coffsip.onrender.com

**Swagger Documentation:** https://coffsip.onrender.com/swagger

**Database Readiness:** https://coffsip.onrender.com/health/ready