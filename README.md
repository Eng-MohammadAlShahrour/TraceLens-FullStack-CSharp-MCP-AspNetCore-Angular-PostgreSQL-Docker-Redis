# TraceLens

**Enterprise Manufacturing Batch Traceability Platform**

*Know the full history of every production batch — from raw material lot to shipment.*

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![C%23](https://img.shields.io/badge/C%23-14-239120)](https://learn.microsoft.com/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192)](https://www.postgresql.org)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED)](https://www.docker.com)
[![OAuth2](https://img.shields.io/badge/OAuth_2.0-OpenIddict-blueviolet)](https://documentation.openiddict.com)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

[Features](#features) · [Architecture](#architecture) · [Quick Start](#quick-start) · [API](#api-reference) · [Security](#security--authentication) · [Design Decisions](#design-decisions)

---

## What is TraceLens?

TraceLens is a full-stack, production-grade platform for manufacturing batch traceability. It tracks the complete lifecycle of a production batch — which raw material lots went into it, quality control results, and shipment history — with a strict state machine, full audit trail, and domain events.

**Business domain:** A CNC workshop produces a batch of 100 precision parts. TraceLens records which aluminum lot (with mill certificate) was used, which quality checks it passed, and which customer received it. If a defect is later discovered, the full chain of custody is one query away.

---

## Features

### Batch State Machine

- 8 states with guarded transitions
- Business rules enforced in `Batch` aggregate (no invalid transition possible)
- Domain events raised at every important transition

### Material Traceability

- Each batch records every material lot it consumed
- Material lots carry `LotNumber`, `SupplierId`, `MaterialType` (Raw / Component / Packaging), and `CertificateOfAnalysis`
- Enforced: cannot add materials after production ends

### Quality Control

- QC pass/fail with automatic status transition
- Failed QC prevents shipping
- Quarantine with required reason (audit-friendly)

### Audit Trail

- Every state change recorded with timestamp
- `CreatedAt`, `UpdatedAt`, `StartedAt`, `CompletedAt` on every aggregate
- Domain events (`BatchCreated`, `QualityCheckPassed`, `BatchQuarantined`)

### Clean Architecture

- 4 layers: Domain -> Application -> Infrastructure -> API
- CQRS with MediatR for every feature
- FluentValidation pipeline behavior (auto-validates every command)
- Rich Domain Model (not anemic — behavior lives in entities)

### Observability

- Serilog structured logging (JSON output)
- OpenAPI auto-generated docs (Microsoft.AspNetCore.OpenApi)
- Swagger UI for interactive testing
- Global exception middleware with typed error responses

---

## Architecture

### 4-Layer Clean Architecture

- **Domain** — Aggregates, Value Objects, Enums, Domain Events. Pure C#, no external dependencies except MediatR.
- **Application** — CQRS, MediatR handlers, FluentValidation, DTOs.
- **Infrastructure** — EF Core 10, Npgsql, ApplicationDbContext, per-entity configurations, migrations.
- **API** — ASP.NET Core 10 controllers, OpenAPI, Serilog, global exception middleware.
- **[Planned] Auth** — OpenIddict OAuth 2.0 server embedded in the API.

### Repository Layout

- `src/TraceLens.Domain/` — Common, Entities, Enums, ValueObjects, Events
- `src/TraceLens.Application/` — Common, Features/Batches (CreateBatch, GetBatchById, GetBatchesList)
- `src/TraceLens.Infrastructure/` — Persistence, Configurations, Migrations
- `src/TraceLens.API/` — Controllers, Middleware, Program.cs
- `tests/TraceLens.Tests/` — xUnit + FluentAssertions

### Database Schema

| Table | Purpose |
| :--- | :--- |
| batches | Batch aggregate root (unique batch_number, status enum) |
| material_lots | Material lots attached to batches (FK batch_id) |
| work_orders | Work orders (unique order_number) |
| __EFMigrationsHistory | EF Core migration tracking |

---

## Quick Start

### Prerequisites

- .NET 10 SDK
- Docker Desktop 4.x+ (with Compose V2)
- Ports 5268 (API), 5433 (PostgreSQL) free

### Run

```bash
git clone https://github.com/Eng-MohammadAlShahrour/TraceLens-FullStack-CSharp-AspNetCore-Angular-PostgreSQL-Docker.git
cd TraceLens-FullStack-CSharp-AspNetCore-Angular-PostgreSQL-Docker
docker compose up -d
dotnet ef database update --project src/TraceLens.Infrastructure --startup-project src/TraceLens.API
dotnet run --project src/TraceLens.API
Note: The container uses port 5433 on the host to avoid conflict with any locally-installed PostgreSQL on port 5432.

URLs
ServiceURL
APIhttp://localhost:5268
Swagger UIhttp://localhost:5268/swagger
OpenAPI JSONhttp://localhost:5268/openapi/v1.json
API Reference
Batches (/api/batches)
MethodPathReturns
POST/api/batches201 Created + CreateBatchResponse
GET/api/batches/{id}200 OK + BatchDto
GET/api/batches?page=1&pageSize=20200 OK + paginated list
Sample Request
bash
curl -X POST http://localhost:5268/api/batches \
  -H "Content-Type: application/json" \
  -d '{"batchNumber":"B-2026-001","workOrderId":"11111111-1111-1111-1111-111111111111","productId":"22222222-2222-2222-2222-222222222222","producedQuantity":100,"producedQuantityUnit":"pcs"}'
Error Responses
StatusWhenBody
400Validation fails{ type: "ValidationError", errors: {...} }
400Domain rule violated{ type: "BadRequest", title: "..." }
404Batch ID not found—
500Unexpected error{ type: "ServerError", title: "..." }
Security & Authentication
Current State (MVP)
The API currently runs without authentication — this is intentional for the initial portfolio demo. Every endpoint is public.

Planned: OAuth 2.0 with OpenIddict
OpenIddict will be embedded in TraceLens.API as a full OAuth 2.0 / OpenID Connect authorization server. This is the recommended approach for .NET projects — it is free, open-source (Apache 2.0), and lets the entire identity layer live inside the same codebase.

Why OpenIddict and not Duende IdentityServer?
Duende requires a paid commercial license for production use

OpenIddict is 100% free for both development and production

Both implement the same OAuth 2.0 / OIDC standards

OpenIddict integrates natively with ASP.NET Core 10 Identity

Planned endpoints
EndpointPurpose
POST /connect/tokenExchange credentials for tokens (PKCE)
GET /connect/authorizeAuthorization endpoint (PKCE flow)
POST /connect/revokeRevoke access / refresh tokens
GET /connect/userinfoOIDC user info endpoint
Planned security features
PKCE (Proof Key for Code Exchange) — required for SPA clients

Refresh tokens with rotation

RBAC — Admin, Technician, Viewer roles

Short-lived access tokens (15 min) + long-lived refresh tokens

Scoped permissions — batches:read, batches:write, batches:approve

Other Security Measures (already in place)
ConcernMitigation
SQL injectionEF Core parameterized queries
Mass assignmentExplicit CQRS commands
Invalid stateRich Domain Model enforces invariants
Error leakingGlobal exception middleware returns generic messages
Secrets in codeappsettings.Development.json for dev only
CORSExplicit origins (localhost:4200, localhost:3000)
Design Decisions
Why Clean Architecture (4 projects)?
Testability + independence from frameworks. The Domain layer contains zero external dependencies (except MediatR for domain events). Business rules can be tested without EF Core, PostgreSQL, or ASP.NET Core.

Why CQRS with MediatR?
Commands and queries have different shapes. A CreateBatchCommand mutates state and returns an ID. A GetBatchesListQuery reads data and returns a paginated DTO. Separating them makes each handler small and testable in isolation.

Why FluentValidation in a pipeline behavior?
Validation is cross-cutting. Every command needs it. The ValidationBehavior runs before every handler automatically. Add a new validator — it is enforced. No changes to the handler.

Why Rich Domain Model (not anemic)?
Invalid state should be impossible. Batch.MarkQCCompleted(result) throws if the batch is still in Created state. You cannot accidentally ship an un-QC'd batch — the domain forbids it.

Why Domain Events?
Decoupling. When a batch passes QC, we might want to send an email, update a dashboard, or trigger a downstream process. Domain events let us add these without touching the Batch aggregate.

Why Value Objects for BatchNumber and Quantity?
Primitive obsession is a bug farm. string batchNumber can be empty or malformed. Quantity can be negative. By using BatchNumber.Create() and Quantity.Create(), invalid values are rejected at construction.

Why PostgreSQL (not SQL Server)?
Cross-platform, JSONB support, pg_trgm for fuzzy search, open source, no licensing headaches.

Why Port 5433 for Docker?
Coexistence with local PostgreSQL. Many developers have PostgreSQL installed on Windows (port 5432). Using 5433 lets TraceLens run alongside without conflict.

Testing
The test project is scaffolded with xUnit + FluentAssertions + EF Core InMemory.

Planned test suites
SuiteTestsCoverage
BatchTests~15All state transitions + guard conditions
BatchNumberTests~5Validation, normalization, equality
QuantityTests~5Negative rejection, unit normalization
CreateBatchHandlerTests~3Happy path + validation failure
GetBatchesListHandlerTests~3Pagination edge cases
OAuthFlowTests~8PKCE, token exchange, refresh, revoke
Roadmap
☑ Clean Architecture — 4 layers, DI, testable
☑ CQRS with MediatR — 3 features (Create, GetById, List)
☑ EF Core + PostgreSQL — migrations, configurations, enums as strings
☑ Domain Model — aggregates, value objects, domain events
☑ REST API — controllers, OpenAPI, Swagger UI
☑ Global Exception Middleware — typed error responses
☑ Docker Compose — PostgreSQL container, port isolation
□ OAuth 2.0 with OpenIddict — authorization server, PKCE, RBAC, refresh tokens
□ SignalR — real-time batch status updates
□ Hangfire — QC reminders, stale batch alerts
□ Redis — caching for dashboard aggregations
□ Angular 20 frontend — Warm Dark + Violet design system
□ Playwright E2E tests — full user journey
Tech Stack
LayerTechnology
Runtime.NET 10 (LTS until Nov 2028)
LanguageC# 14
APIASP.NET Core 10, Controllers
OpenAPIMicrosoft.AspNetCore.OpenApi 10
Swagger UISwashbuckle.AspNetCore.SwaggerUI 10
CQRSMediatR 14
ValidationFluentValidation 12
ORMEF Core 10
DatabasePostgreSQL 16 (Alpine)
LoggingSerilog
Auth (planned)OpenIddict (OAuth 2.0 + OIDC)
TestsxUnit, FluentAssertions, EF Core InMemory
ContainerDocker Compose
License
MIT (c) 2026 Mohammad Al-Shahrour

Built with coffee and a healthy obsession with traceability.
