<p align="center">
  <img src="docs/screenshots/hero.png" alt="TraceLens — manufacturing batch traceability" width="100%" />
</p>

<div align="center">

# TraceLens

### Manufacturing batch traceability, end to end

A full-stack **C# / ASP.NET Core 10** platform that tracks every production
batch — which raw material lots went into it, which quality checks it passed,
and which customer received it. Strict state machine, full audit trail,
and domain events.

<br/>

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![C#](https://img.shields.io/badge/C%23-14-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![EF Core](https://img.shields.io/badge/EF_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core)
[![MediatR](https://img.shields.io/badge/MediatR-14-6A1B9A?style=for-the-badge)](https://github.com/jbogard/MediatR)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://docker.com)
[![OpenIddict](https://img.shields.io/badge/OAuth_2.0-OpenIddict-blueviolet?style=for-the-badge)](https://documentation.openiddict.com)

</div>

---

## What it does

| Feature | How |
|---|---|
| **Batch lifecycle** | Created → InProduction → QCPending → QCPassed → Shipped |
| **Alternative branches** | QCFailed and Quarantined |
| **Material traceability** | Every batch records the material lots it consumed |
| **Quality control** | Pass/fail with automatic status transition |
| **Audit trail** | Every state change with timestamp on every aggregate |
| **Domain events** | BatchCreated, QualityCheckPassed, BatchQuarantined |
| **Rich domain model** | Invalid transitions throw — the domain forbids them |
| **Clean architecture** | Domain → Application → Infrastructure → API |
| **CQRS** | MediatR handlers + FluentValidation pipeline behavior |
| **Observability** | Serilog + OpenAPI + Swagger UI |
| **OAuth 2.0 (planned)** | OpenIddict embedded, PKCE, RBAC, refresh tokens |

---

## Quick Start

Prerequisites: **.NET 10 SDK**, **Docker Desktop 4.x+** with Compose V2.

    git clone https://github.com/Eng-MohammadAlShahrour/TraceLens-FullStack-CSharp-AspNetCore-Angular-PostgreSQL-Docker.git
    cd TraceLens-FullStack-CSharp-AspNetCore-Angular-PostgreSQL-Docker

    docker compose up -d
    dotnet ef database update --project src/TraceLens.Infrastructure --startup-project src/TraceLens.API
    dotnet run --project src/TraceLens.API

> **Note:** The container uses port **5433** on the host to avoid conflict with any locally-installed PostgreSQL on port 5432.

### URLs

| Service | URL |
|---|---|
| **API** | http://localhost:5268 |
| **Swagger UI** | http://localhost:5268/swagger |
| **OpenAPI JSON** | http://localhost:5268/openapi/v1.json |

---

## Architecture

### 4-Layer Clean Architecture

| Layer | Purpose | Tech |
|---|---|---|
| **Domain** | Aggregates, Value Objects, Enums, Domain Events | Pure C# |
| **Application** | CQRS, MediatR handlers, FluentValidation, DTOs | MediatR 14 |
| **Infrastructure** | ApplicationDbContext, configurations, migrations | EF Core 10 + Npgsql |
| **API** | Controllers, middleware, OpenAPI, Serilog | ASP.NET Core 10 |
| **[Planned] Auth** | OAuth 2.0 / OIDC authorization server | OpenIddict |

### Repository Layout

    src/
    ├── TraceLens.Domain/
    │   ├── Common/          BaseEntity, AggregateRoot, IDomainEvent
    │   ├── Entities/        Batch, MaterialLot, WorkOrder
    │   ├── Enums/           BatchStatus, QualityCheckResult, MaterialType
    │   ├── ValueObjects/    BatchNumber, Quantity
    │   └── Events/          BatchCreated, QualityCheckPassed, BatchQuarantined
    ├── TraceLens.Application/
    │   ├── Common/
    │   │   ├── Interfaces/  IApplicationDbContext
    │   │   └── Behaviors/   ValidationBehavior<,>
    │   └── Features/Batches/
    │       ├── CreateBatch/
    │       ├── GetBatchById/
    │       └── GetBatchesList/
    ├── TraceLens.Infrastructure/
    │   ├── Persistence/
    │   │   ├── ApplicationDbContext
    │   │   ├── Configurations/
    │   │   └── Migrations/
    │   └── DependencyInjection
    └── TraceLens.API/
        ├── Controllers/     BatchesController
        ├── Middleware/      GlobalExceptionMiddleware
        └── Program.cs

### Database Schema

| Table | Purpose |
|---|---|
| **batches** | Batch aggregate root (unique batch_number, status enum) |
| **material_lots** | Material lots attached to batches (FK batch_id) |
| **work_orders** | Work orders (unique order_number) |
| **__EFMigrationsHistory** | EF Core migration tracking |

---

## API Reference

### Batches (`/api/batches`)

| Method | Path | Body | Returns |
|---|---|---|---|
| POST | `/api/batches` | `{ batchNumber, workOrderId, productId, producedQuantity, producedQuantityUnit }` | `201 Created` + `CreateBatchResponse` |
| GET | `/api/batches/{id}` | — | `200 OK` + `BatchDto` |
| GET | `/api/batches?page=1&pageSize=20` | — | `200 OK` + paginated list |

### Sample Request

    curl -X POST http://localhost:5268/api/batches \
      -H "Content-Type: application/json" \
      -d '{
        "batchNumber": "B-2026-001",
        "workOrderId": "11111111-1111-1111-1111-111111111111",
        "productId":  "22222222-2222-2222-2222-222222222222",
        "producedQuantity": 100,
        "producedQuantityUnit": "pcs"
      }'

### Error Responses

| Status | When | Body |
|---|---|---|
| `400` | Validation fails | `{ type: "ValidationError", errors: { Field: [messages] } }` |
| `400` | Domain rule violated | `{ type: "BadRequest", title: "..." }` |
| `404` | Batch ID not found | — |
| `500` | Unexpected error | `{ type: "ServerError", title: "..." }` |

---

## Security & Authentication

### Current State (MVP)

The API runs **without authentication** — intentional for the initial portfolio demo. Every endpoint is public.

### Planned: OAuth 2.0 with OpenIddict

**OpenIddict** will be embedded in `TraceLens.API` as a full **OAuth 2.0 / OpenID Connect** authorization server. Free, open-source (Apache 2.0), and lives inside the same codebase.

**Why OpenIddict and not Duende IdentityServer?**

- Duende requires a **paid commercial license** for production use
- OpenIddict is **100% free** for both development and production
- Both implement the same OAuth 2.0 / OIDC standards
- OpenIddict integrates natively with ASP.NET Core 10 Identity

### Planned Endpoints

| Endpoint | Purpose |
|---|---|
| `POST /connect/token` | Exchange credentials for tokens (PKCE) |
| `GET /connect/authorize` | Authorization endpoint (PKCE flow) |
| `POST /connect/revoke` | Revoke access / refresh tokens |
| `GET /connect/userinfo` | OIDC user info endpoint |

### Planned Security Features

- **PKCE** (Proof Key for Code Exchange) — required for SPA clients
- **Refresh tokens** with rotation
- **RBAC** — Admin, Technician, Viewer roles
- **Short-lived access tokens** (15 min) + long-lived refresh tokens
- **Scoped permissions** — `batches:read`, `batches:write`, `batches:approve`

### Other Security Measures (already in place)

| Concern | Mitigation |
|---|---|
| SQL injection | EF Core parameterized queries |
| Mass assignment | Explicit CQRS commands (no auto-binding to entities) |
| Invalid state | Rich Domain Model enforces invariants |
| Error leaking | Global exception middleware returns generic messages |
| Secrets in code | appsettings.Development.json for dev only |
| CORS | Explicit origins (localhost:4200, localhost:3000) |

---

## Design Decisions

**Why Clean Architecture (4 projects)?**
Testability + independence from frameworks. The Domain layer has zero external dependencies except MediatR. Business rules can be tested without EF Core, PostgreSQL, or ASP.NET Core.

**Why CQRS with MediatR?**
Commands and queries have different shapes. `CreateBatchCommand` mutates state and returns an ID. `GetBatchesListQuery` reads data and returns a paginated DTO. Each handler stays small and testable in isolation.

**Why FluentValidation in a pipeline behavior?**
Validation is cross-cutting. `ValidationBehavior<,>` runs before every handler automatically. Add a validator — it is enforced. No changes to the handler.

**Why Rich Domain Model (not anemic)?**
Invalid state should be impossible. `Batch.MarkQCCompleted(result)` throws if the batch is still in `Created` state. You cannot accidentally ship an un-QC'd batch — the domain forbids it.

**Why Domain Events?**
Decoupling. When a batch passes QC, we might want to send an email, update a dashboard, or trigger a downstream process. Domain events let us add these without touching the `Batch` aggregate.

**Why Value Objects for BatchNumber and Quantity?**
Primitive obsession is a bug farm. `string batchNumber` can be empty or malformed. `Quantity` can be negative. `BatchNumber.Create()` and `Quantity.Create()` reject invalid values at construction.

**Why PostgreSQL (not SQL Server)?**
Cross-platform. JSONB for future metadata. `pg_trgm` for fuzzy search on material names. Open source, no licensing headaches.

**Why Port 5433 for Docker?**
Coexistence with local PostgreSQL. Many developers have PostgreSQL on Windows (port 5432). Using 5433 lets TraceLens run alongside without conflict.

---

## Testing

The test project is scaffolded with xUnit + FluentAssertions + EF Core InMemory.

### Planned Test Suites

| Suite | Tests | Coverage |
|---|---|---|
| **BatchTests** | ~15 | All state transitions + guard conditions |
| **BatchNumberTests** | ~5 | Validation, normalization, equality |
| **QuantityTests** | ~5 | Negative rejection, unit normalization |
| **CreateBatchHandlerTests** | ~3 | Happy path + validation failure |
| **GetBatchesListHandlerTests** | ~3 | Pagination edge cases |
| **OAuthFlowTests** | ~8 | PKCE, token exchange, refresh, revoke |

---

## Tech Stack

| Layer | Technology |
|---|---|
| **Runtime** | .NET 10 (LTS until Nov 2028) |
| **Language** | C# 14 |
| **API** | ASP.NET Core 10, Controllers |
| **OpenAPI** | Microsoft.AspNetCore.OpenApi 10 |
| **Swagger UI** | Swashbuckle.AspNetCore.SwaggerUI 10 |
| **CQRS** | MediatR 14 |
| **Validation** | FluentValidation 12 |
| **ORM** | EF Core 10 |
| **Database** | PostgreSQL 16 (Alpine) |
| **Logging** | Serilog |
| **Auth (planned)** | OpenIddict (OAuth 2.0 + OIDC) |
| **Tests** | xUnit, FluentAssertions, EF Core InMemory |
| **Container** | Docker Compose |

---

## Roadmap

- [x] **Clean Architecture** — 4 layers, DI, testable
- [x] **CQRS with MediatR** — 3 features (Create, GetById, List)
- [x] **EF Core + PostgreSQL** — migrations, configurations, enums as strings
- [x] **Domain Model** — aggregates, value objects, domain events
- [x] **REST API** — controllers, OpenAPI, Swagger UI
- [x] **Global Exception Middleware** — typed error responses
- [x] **Docker Compose** — PostgreSQL container, port isolation
- [ ] **OAuth 2.0 with OpenIddict** — authorization server, PKCE, RBAC, refresh tokens
- [ ] **SignalR** — real-time batch status updates
- [ ] **Hangfire** — QC reminders, stale batch alerts, scheduled reports
- [ ] **Redis** — caching for dashboard aggregations
- [ ] **Angular 20 frontend** — Warm Dark + Violet design system
- [ ] **Playwright E2E tests** — full user journey

---

## License

MIT © 2026 Mohammad Al-Shahrour

---

*Built with C# / ASP.NET Core, Angular, PostgreSQL — and an unhealthy obsession with traceability.*
