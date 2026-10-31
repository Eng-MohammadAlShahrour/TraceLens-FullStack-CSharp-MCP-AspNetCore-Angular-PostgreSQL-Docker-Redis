# TraceLens

**Manufacturing batch traceability — end to end**

A full-stack **C# / ASP.NET Core 10 + Angular 20** platform that tracks every production batch — which raw material lots went into it, which quality checks it passed, and which customer received it. Strict state machine, full audit trail, domain events, and 59 automated tests.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![C#](https://img.shields.io/badge/C%23-14-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![Angular](https://img.shields.io/badge/Angular-20-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.dev)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://docker.com)
[![Tests](https://img.shields.io/badge/Tests-59_passing-success?style=for-the-badge)](tests/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

---

## What it does

| Feature | How |
| :--- | :--- |
| **Batch lifecycle** | Created → InProduction → QCPending → QCPassed → Shipped |
| **Alternative branches** | QCFailed, Quarantined |
| **Material traceability** | Every batch records the material lots it consumed |
| **Quality control** | Pass/fail with automatic status transition |
| **Audit trail** | Every state change with timestamp |
| **Domain events** | BatchCreated, QualityCheckPassed, BatchQuarantined |
| **Rich domain model** | Invalid transitions throw at the domain level |
| **Clean architecture** | Domain → Application → Infrastructure → API |
| **CQRS** | MediatR handlers + FluentValidation pipeline |
| **Angular 20 frontend** | Dashboard, Batches list, Create form, Details with state machine actions, Quarantine modal |
| **Observability** | Serilog + OpenAPI + Swagger UI |
| **OAuth 2.0 (planned)** | OpenIddict embedded, PKCE, RBAC, refresh tokens |

---

## Screenshots

| Dashboard | Batches List |
| :---: | :---: |
| ![Dashboard](docs/screenshots/dashboard.png) | ![Batches](docs/screenshots/batches.png) |

| Create Batch | Batch Details |
| :---: | :---: |
| ![Create](docs/screenshots/create.png) | ![Details](docs/screenshots/details.png) |

---

## Quick Start

**Prerequisites:** .NET 10 SDK, Node 20+, Docker Desktop 4.x+, Angular CLI 20+.

```bash
# 1. Start PostgreSQL
docker compose up -d

# 2. Run the API
dotnet run --project src/TraceLens.API

# 3. Run the frontend (new terminal)
cd frontend
npm install
npm start
URLs
ServiceURL
APIhttp://localhost:5268
Swagger UIhttp://localhost:5268/swagger
Angularhttp://localhost:4200
Container uses port 5433 on the host to avoid conflict with any local PostgreSQL.

Architecture
4-Layer Clean Architecture
LayerPurposeTech
DomainAggregates, Value Objects, Enums, Domain EventsPure C#
ApplicationCQRS commands/queries, MediatR handlers, FluentValidationMediatR 14
InfrastructureApplicationDbContext, configurations, migrations, seederEF Core 10 + Npgsql
APIControllers, middleware, OpenAPI, SerilogASP.NET Core 10
FrontendDashboard, Batches, Create, DetailsAngular 20 + Tailwind v4
[Planned] AuthOAuth 2.0 / OIDC authorization serverOpenIddict
Domain Model
text
Batch (AggregateRoot)
├── BatchNumber (ValueObject)
├── Quantity (ValueObject) — OwnsOne(value, unit)
├── BatchStatus (enum: Created → InProduction → QCPassed → Shipped)
├── MaterialLots (collection of MaterialLot)
├── WorkOrderId, ProductId
├── Timestamps (Created, Started, Completed)
└── Domain Events (BatchCreated, QualityCheckPassed, BatchQuarantined)
API Reference
Batches (/api/batches) — 8 endpoints
MethodPathDescription
POST/api/batchesCreate a new batch
GET/api/batches/{id}Get batch by ID
GET/api/batches?page=1&pageSize=20Paginated list
POST/api/batches/{id}/start-productionCreated → InProduction
POST/api/batches/{id}/pass-qcInProduction → QCPassed
POST/api/batches/{id}/fail-qcInProduction → QCFailed
POST/api/batches/{id}/shipQCPassed → Shipped
POST/api/batches/{id}/quarantineany → Quarantined (requires reason)
Testing — 59 tests passing
bash
dotnet test
SuiteTestsCoverage
Domain (Batch, BatchNumber, Quantity)31State transitions, guard conditions, ValueObject validation
Application (Validators, Handlers)18CQRS commands, validation pipeline, pagination
API (Controllers)6HTTP responses, mediator wiring
Integration (SQLite)4OwnsOne round-trip, unique indexes, enum-as-string
Design Decisions
Why Clean Architecture (4 projects)?
Testability + independence from frameworks. Domain has zero external dependencies except MediatR. Business rules tested without EF Core, PostgreSQL, or ASP.NET Core.

Why CQRS with MediatR?
Commands and queries have different shapes. CreateBatchCommand mutates and returns an ID. GetBatchesListQuery reads and returns paginated DTO. Each handler stays small and testable.

Why Rich Domain Model (not anemic)?
Invalid state should be impossible. Batch.MarkQCCompleted(result) throws if batch is in Created state. You cannot ship an un-QC'd batch — the domain forbids it.

Why Value Objects for BatchNumber and Quantity?
Primitive obsession is a bug farm. string batchNumber can be empty or malformed. Quantity can be negative. BatchNumber.Create() and Quantity.Create() reject invalid values at construction.

Why OwnsOne for Quantity?
Quantity has Value (decimal) + Unit (string). Using OwnsOne, EF Core stores them as two columns (produced_quantity_value, produced_quantity_unit) in the same table — no extra join, no loss of the unit.

Why Domain Events?
Decoupling. When a batch passes QC, we can send an email, update a dashboard, or trigger a downstream process — without touching the Batch aggregate.

Tech Stack
LayerTechnology
Runtime.NET 10 (LTS until Nov 2028)
LanguageC# 14
APIASP.NET Core 10, Controllers
OpenAPIMicrosoft.AspNetCore.OpenApi 10 + Swashbuckle SwaggerUI
CQRSMediatR 14
ValidationFluentValidation 12
ORMEF Core 10
DatabasePostgreSQL 16 (Alpine)
LoggingSerilog
FrontendAngular 20 + Tailwind CSS v4 + Inter + JetBrains Mono
Design SystemFresh Teal (Custom theme, mobile-first)
Auth (planned)OpenIddict (OAuth 2.0 + OIDC)
TestsxUnit, FluentAssertions, EF Core InMemory, SQLite
ContainerDocker Compose
Roadmap
☑ Clean Architecture — 4 layers, DI, testable
☑ CQRS with MediatR — 8 features
☑ EF Core + PostgreSQL — migrations, OwnsOne, enums as strings
☑ Domain Model — aggregates, value objects, domain events
☑ REST API — controllers, OpenAPI, Swagger UI
☑ Global Exception Middleware — typed error responses
☑ Docker Compose — PostgreSQL container, port isolation
☑ Angular 20 frontend — Dashboard, Batches, Create, Details, Quarantine modal
☑ 59 automated tests — Domain, Application, API, Integration
□ OAuth 2.0 with OpenIddict — PKCE, RBAC, refresh tokens
□ SignalR — real-time batch status updates
□ Hangfire — QC reminders, stale batch alerts
□ Redis — caching for dashboard aggregations
□ Playwright E2E tests — full user journey
License
MIT © 2026 Mohammad Al-Shahrour

Built with C# / ASP.NET Core, Angular, PostgreSQL — and an unhealthy obsession with traceability.
