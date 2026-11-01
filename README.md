# TraceLens

**Manufacturing batch traceability — end to end**

A full-stack **C# / ASP.NET Core 10 + Angular 20** platform that tracks every production batch — which raw material lots went into it, which quality checks it passed, and which customer received it. Strict state machine, full audit trail, domain events, and **59 automated tests**.

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
Service	URL
API	http://localhost:5268
Swagger UI	http://localhost:5268/swagger
Angular	http://localhost:4200
Container uses port 5433 on the host to avoid conflict with any local PostgreSQL.

Architecture
4-Layer Clean Architecture
Layer	Purpose	Tech
Domain	Aggregates, Value Objects, Enums, Domain Events	Pure C#
Application	CQRS commands/queries, MediatR handlers, FluentValidation	MediatR 14
Infrastructure	ApplicationDbContext, configurations, migrations, seeder	EF Core 10 + Npgsql
API	Controllers, middleware, OpenAPI, Serilog	ASP.NET Core 10
Frontend	Dashboard, Batches, Create, Details	Angular 20 + Tailwind v4
[Planned] Auth	OAuth 2.0 / OIDC authorization server	OpenIddict
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
Method	Path	Description
POST	/api/batches	Create a new batch
GET	/api/batches/{id}	Get batch by ID
GET	/api/batches?page=1&pageSize=20	Paginated list
POST	/api/batches/{id}/start-production	Created → InProduction
POST	/api/batches/{id}/pass-qc	InProduction → QCPassed
POST	/api/batches/{id}/fail-qc	InProduction → QCFailed
POST	/api/batches/{id}/ship	QCPassed → Shipped
POST	/api/batches/{id}/quarantine	any → Quarantined (requires reason)
Testing — 59 tests passing
bash
dotnet test
Suite	Tests	Coverage
Domain (Batch, BatchNumber, Quantity)	31	State transitions, guard conditions, ValueObject validation
Application (Validators, Handlers)	18	CQRS commands, validation pipeline, pagination
API (Controllers)	6	HTTP responses, mediator wiring
Integration (SQLite)	4	OwnsOne round-trip, unique indexes, enum-as-string
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
Layer	Technology
Runtime	.NET 10 (LTS until Nov 2028)
Language	C# 14
API	ASP.NET Core 10, Controllers
OpenAPI	Microsoft.AspNetCore.OpenApi 10 + Swashbuckle SwaggerUI
CQRS	MediatR 14
Validation	FluentValidation 12
ORM	EF Core 10
Database	PostgreSQL 16 (Alpine)
Logging	Serilog
Frontend	Angular 20 + Tailwind CSS v4 + Inter + JetBrains Mono
Design System	Fresh Teal (Custom theme, mobile-first)
Auth (planned)	OpenIddict (OAuth 2.0 + OIDC)
Tests	xUnit, FluentAssertions, EF Core InMemory, SQLite
Container	Docker Compose
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

Built with C# / ASP.NET Core, Angular, PostgreSQL
# TraceLens

**Manufacturing batch traceability — end to end**

A full-stack **C# / ASP.NET Core 10 + Angular 20** platform that tracks every production batch — which raw material lots went into it, which quality checks it passed, and which customer received it. Strict state machine, full audit trail, domain events, and **59 automated tests**.

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
```

### URLs

| Service | URL |
| :--- | :--- |
| **API** | http://localhost:5268 |
| **Swagger UI** | http://localhost:5268/swagger |
| **Angular** | http://localhost:4200 |

> Container uses port **5433** on the host to avoid conflict with any local PostgreSQL.

---

## Architecture

### 4-Layer Clean Architecture

| Layer | Purpose | Tech |
| :--- | :--- | :--- |
| **Domain** | Aggregates, Value Objects, Enums, Domain Events | Pure C# |
| **Application** | CQRS commands/queries, MediatR handlers, FluentValidation | MediatR 14 |
| **Infrastructure** | ApplicationDbContext, configurations, migrations, seeder | EF Core 10 + Npgsql |
| **API** | Controllers, middleware, OpenAPI, Serilog | ASP.NET Core 10 |
| **Frontend** | Dashboard, Batches, Create, Details | Angular 20 + Tailwind v4 |
| **[Planned] Auth** | OAuth 2.0 / OIDC authorization server | OpenIddict |

### Domain Model

```
Batch (AggregateRoot)
├── BatchNumber (ValueObject)
├── Quantity (ValueObject) — OwnsOne(value, unit)
├── BatchStatus (enum: Created → InProduction → QCPassed → Shipped)
├── MaterialLots (collection of MaterialLot)
├── WorkOrderId, ProductId
├── Timestamps (Created, Started, Completed)
└── Domain Events (BatchCreated, QualityCheckPassed, BatchQuarantined)
```

---

## API Reference

### Batches (`/api/batches`) — 8 endpoints

| Method | Path | Description |
| :--- | :--- | :--- |
| POST | `/api/batches` | Create a new batch |
| GET | `/api/batches/{id}` | Get batch by ID |
| GET | `/api/batches?page=1&pageSize=20` | Paginated list |
| POST | `/api/batches/{id}/start-production` | Created → InProduction |
| POST | `/api/batches/{id}/pass-qc` | InProduction → QCPassed |
| POST | `/api/batches/{id}/fail-qc` | InProduction → QCFailed |
| POST | `/api/batches/{id}/ship` | QCPassed → Shipped |
| POST | `/api/batches/{id}/quarantine` | any → Quarantined (requires reason) |

---
## MCP Server (Model Context Protocol)

TraceLens exposes a **Model Context Protocol** server over HTTP at `/mcp`. AI agents (Claude Desktop, Cursor, VS Code, etc.) can call 3 tools backed by the same CQRS handlers and PostgreSQL data.

| Tool | Purpose | Parameters |
| :--- | :--- | :--- |
| `get_batches_by_status` | Filter batches by lifecycle status | `status` (string) |
| `get_work_orders` | List all production work orders | — |
| `create_batch` | Create a new batch | `batchNumber`, `workOrderId`, `productId`, `producedQuantity`, `producedQuantityUnit` |

### Connect via MCP Inspector

```bash
npx @modelcontextprotocol/inspector --url http://localhost:5268/mcp
Call a tool (JSON-RPC)
bash
curl -X POST http://localhost:5268/mcp \
  -H "Content-Type: application/json" \
  -H "Accept: application/json, text/event-stream" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"get_work_orders","arguments":{}}}'
Why this is a real AI integration
No duplicate logic — every tool calls the same ISender (MediatR) used by REST endpoints.

No fake data — tools query PostgreSQL via EF Core.

Clean Architecture preserved — MCP is just another entry point, not a new project.

## Testing — 59 tests passing

```bash
dotnet test
```

| Suite | Tests | Coverage |
| :--- | :--- | :--- |
| **Domain** (Batch, BatchNumber, Quantity) | 31 | State transitions, guard conditions, ValueObject validation |
| **Application** (Validators, Handlers) | 18 | CQRS commands, validation pipeline, pagination |
| **API** (Controllers) | 6 | HTTP responses, mediator wiring |
| **Integration** (SQLite) | 4 | OwnsOne round-trip, unique indexes, enum-as-string |

---

## Design Decisions

### Why Clean Architecture (4 projects)?
Testability + independence from frameworks. Domain has zero external dependencies except MediatR. Business rules tested without EF Core, PostgreSQL, or ASP.NET Core.

### Why CQRS with MediatR?
Commands and queries have different shapes. `CreateBatchCommand` mutates and returns an ID. `GetBatchesListQuery` reads and returns paginated DTO. Each handler stays small and testable.

### Why Rich Domain Model (not anemic)?
Invalid state should be impossible. `Batch.MarkQCCompleted(result)` throws if batch is in `Created` state. You cannot ship an un-QC'd batch — the domain forbids it.

### Why Value Objects for BatchNumber and Quantity?
Primitive obsession is a bug farm. `string batchNumber` can be empty or malformed. `Quantity` can be negative. `BatchNumber.Create()` and `Quantity.Create()` reject invalid values at construction.

### Why OwnsOne for Quantity?
`Quantity` has `Value` (decimal) + `Unit` (string). Using `OwnsOne`, EF Core stores them as two columns (`produced_quantity_value`, `produced_quantity_unit`) in the same table — no extra join, no loss of the unit.

### Why Domain Events?
Decoupling. When a batch passes QC, we can send an email, update a dashboard, or trigger a downstream process — without touching the `Batch` aggregate.

---

## Tech Stack

| Layer | Technology |
| :--- | :--- |
| **Runtime** | .NET 10 (LTS until Nov 2028) |
| **Language** | C# 14 |
| **API** | ASP.NET Core 10, Controllers |
| **OpenAPI** | Microsoft.AspNetCore.OpenApi 10 + Swashbuckle SwaggerUI |
| **CQRS** | MediatR 14 |
| **Validation** | FluentValidation 12 |
| **ORM** | EF Core 10 |
| **Database** | PostgreSQL 16 (Alpine) |
| **Logging** | Serilog |
| **Frontend** | Angular 20 + Tailwind CSS v4 + Inter + JetBrains Mono |
| **Design System** | Fresh Teal (Custom theme, mobile-first) |
| **Auth (planned)** | OpenIddict (OAuth 2.0 + OIDC) |
| **Tests** | xUnit, FluentAssertions, EF Core InMemory, SQLite |
| **Container** | Docker Compose |

---

## Roadmap

- [x] Clean Architecture — 4 layers, DI, testable
- [x] CQRS with MediatR — 8 features
- [x] EF Core + PostgreSQL — migrations, OwnsOne, enums as strings
- [x] Domain Model — aggregates, value objects, domain events
- [x] REST API — controllers, OpenAPI, Swagger UI
- [x] Global Exception Middleware — typed error responses
- [x] Docker Compose — PostgreSQL container, port isolation
- [x] Angular 20 frontend — Dashboard, Batches, Create, Details, Quarantine modal
- [x] 59 automated tests — Domain, Application, API, Integration
- [ ] OAuth 2.0 with OpenIddict — PKCE, RBAC, refresh tokens
- [ ] SignalR — real-time batch status updates
- [ ] Hangfire — QC reminders, stale batch alerts
- [ ] Redis — caching for dashboard aggregations
- [ ] Playwright E2E tests — full user journey

---

## License

MIT © 2026 Mohammad Al-Shahrour

---

*Built with C# / ASP.NET Core, Angular, PostgreSQL.*
