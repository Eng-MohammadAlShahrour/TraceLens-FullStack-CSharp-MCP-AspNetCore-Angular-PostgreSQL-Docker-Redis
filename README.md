# TraceLens

> Manufacturing batch traceability — end to end

A full-stack **C# / ASP.NET Core 10 + Angular 20** platform that tracks every production batch — which raw material lots went into it, which quality checks it passed, and which customer received it. Strict state machine, full audit trail, domain events, real-time updates, **OAuth 2.0 authentication**, 87 automated tests, and an **MCP server for AI agents**.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![C#](https://img.shields.io/badge/C%23-14-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![Angular](https://img.shields.io/badge/Angular-20-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.dev)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://docker.com)
[![MCP](https://img.shields.io/badge/MCP-Model_Context_Protocol-blueviolet?style=for-the-badge)](https://modelcontextprotocol.io)
[![OAuth](https://img.shields.io/badge/OAuth-2.0-blueviolet?style=for-the-badge&logo=openid)](https://openid.net/connect/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-purple?style=for-the-badge)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![Tests](https://img.shields.io/badge/Tests-87_passing-success?style=for-the-badge)](tests/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

---

## What it does

| Feature | How |
|---|---|
| **Batch lifecycle** | Created → InProduction → QCPending → QCPassed → Shipped |
| **Alternative branches** | QCFailed, Quarantined |
| **Material traceability** | Every batch records the material lots it consumed |
| **Quality control** | Pass/fail with automatic status transition |
| **Audit trail** | Every state change with timestamp |
| **Domain events** | BatchCreated, QualityCheckPassed, BatchQuarantined |
| **Rich domain model** | Invalid transitions throw at the domain level |
| **Clean architecture** | Domain → Application → Infrastructure → API |
| **CQRS** | MediatR handlers + FluentValidation pipeline |
| **Angular 20 frontend** | Dashboard (real API stats), Batches, Create, Details, Work Orders, MCP Playground |
| **Dashboard KPIs** | Live counts from PostgreSQL — Total, InProduction, QCPassed, Quarantined, QC success rate |
| **SignalR real-time** | Live dashboard + batches updates on every new batch (no refresh) |
| **OAuth 2.0 (OpenIddict)** | Embedded authorization server — password grant + refresh tokens, JWE-encrypted tokens |
| **MCP server** | 3 tools for AI agents (Claude Desktop, Cursor, VS Code) |
| **MCP Playground** | Built-in Angular UI to test all 3 MCP tools live from the browser |
| **Observability** | Serilog + OpenAPI + Swagger UI |

---

## Quick Start

**Prerequisites:** .NET 10 SDK, Node 20+, Docker Desktop 4.x+, Angular CLI 20+.

```bash
git clone https://github.com/Eng-MohammadAlShahrour/TraceLens-FullStack-CSharp-MCP-AspNetCore-Angular-PostgreSQL-Docker.git
cd TraceLens-FullStack-CSharp-MCP-AspNetCore-Angular-PostgreSQL-Docker
```

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

### Login

Open `http://localhost:4200` → you'll be redirected to `/login`.

| Field | Value |
|---|---|
| **Username** | `admin` |
| **Password** | `Admin123!` |

### URLs

| Service | URL |
|---|---|
| API | http://localhost:5268 |
| Swagger UI | http://localhost:5268/swagger |
| Angular | http://localhost:4200 |
| MCP | http://localhost:5268/mcp |
| OAuth Token Endpoint | http://localhost:5268/connect/token |

> Container uses port **5433** on the host to avoid conflict with any local PostgreSQL.

---

## Architecture

### 4-Layer Clean Architecture

| Layer | Purpose | Tech |
|---|---|---|
| **Domain** | Aggregates, Value Objects, Enums, Domain Events | Pure C# |
| **Application** | CQRS commands/queries, MediatR handlers, FluentValidation | MediatR 14 |
| **Infrastructure** | ApplicationDbContext, configurations, migrations, seeder | EF Core 10 + Npgsql |
| **API** | Controllers, middleware, OpenAPI, Serilog, MCP tools, SignalR hubs | ASP.NET Core 10 |
| **Frontend** | Dashboard, Batches, Create, Details, MCP Playground, Login | Angular 20 + Tailwind v4 |
| **Auth** | OAuth 2.0 / OIDC authorization server | OpenIddict 7.x |

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

All endpoints require `Authorization: Bearer <token>` (OAuth 2.0 access token).

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/batches` | Create a new batch |
| `GET` | `/api/batches/{id}` | Get batch by ID |
| `GET` | `/api/batches?page=1&pageSize=20` | Paginated list |
| `POST` | `/api/batches/{id}/start-production` | Created → InProduction |
| `POST` | `/api/batches/{id}/pass-qc` | InProduction → QCPassed |
| `POST` | `/api/batches/{id}/fail-qc` | InProduction → QCFailed |
| `POST` | `/api/batches/{id}/ship` | QCPassed → Shipped |
| `POST` | `/api/batches/{id}/quarantine` | any → Quarantined (requires reason) |

### Work Orders (`/api/work-orders`) — 2 endpoints

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/work-orders` | Create a work order |
| `GET` | `/api/work-orders?page=1&pageSize=20` | Paginated list |

### Auth (`/connect/token`) — OAuth 2.0

See the [OAuth 2.0 section](#oauth-20-openiddict) below.

---

## OAuth 2.0 (OpenIddict)

TraceLens includes an **embedded OAuth 2.0 / OpenID Connect authorization server** using [OpenIddict 7.x](https://documentation.openiddict.com) — no external identity provider required.

### Token endpoint

`POST /connect/token` (`application/x-www-form-urlencoded`)

| Grant type | Credentials | Returns |
|---|---|---|
| `password` | `username` + `password` + `client_id` + `client_secret` | `access_token` (JWE), `token_type: Bearer`, `expires_in` |
| `refresh_token` | `refresh_token` + `client_id` + `client_secret` | new `access_token` |

### Demo client

| Field | Value |
|---|---|
| `client_id` | `tracelens-spa` |
| `client_secret` | `tracelens-secret` |
| `scope` | `api` |

### Demo user

| Field | Value |
|---|---|
| `username` | `admin` |
| `password` | `Admin123!` |
| `role` | `Admin` |

### Try it

```bash
curl -X POST http://localhost:5268/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=admin&password=Admin123!&client_id=tracelens-spa&client_secret=tracelens-secret&scope=api"
```

Response:

```json
{
  "access_token": "eyJhbGciOiJSU0EtT0FFUCIs...",
  "token_type": "Bearer",
  "expires_in": 3599,
  "username": "admin",
  "display_name": "Mohammad Al-Shahrour",
  "role": "Admin"
}
```

### Protected endpoints

All `/api/batches/*` endpoints require `Authorization: Bearer <token>`:

```bash
curl http://localhost:5268/api/batches \
  -H "Authorization: Bearer <access_token>"
```

Missing/invalid token → `401 Unauthorized`.

### Why this matters

- **Real OAuth 2.0 server** — not JWT-only. Token issuance, validation, and refresh all flow through OpenIddict.
- **JWE-encrypted tokens** — access tokens use RSA-OAEP + A256CBC-HS512, so a leaked token can't be decoded by third parties.
- **Angular integration** — the SPA calls `/connect/token` directly, stores the JWE, and attaches it via a functional HTTP interceptor.
- **Clean Architecture preserved** — `IAuthService` lives in Application; `JwtAuthService` + OpenIddict config live in Infrastructure/API.
- **Graceful fallback** — the legacy JWT scheme remains wired alongside OpenIddict so existing callers don't break during migration.

---

## Real-time updates (SignalR)

Every batch created — from REST, MCP, or the Angular UI — broadcasts a `BatchCreated` event over SignalR (WebSockets) at `/hubs/batches`.

The Angular frontend:
- **Auto-reconnects** with exponential backoff
- **Refreshes the dashboard KPIs** live — no manual refresh needed
- **Prepends new batches** to the batches list as they arrive

### Try it

1. Open `http://localhost:4200/dashboard` in one tab
2. Open `http://localhost:4200/mcp` in another tab
3. In the MCP Playground, execute `create_batch` with any batch number
4. Watch the **dashboard update live** (Total Batches increments, new batch appears in Recent Batches — within ~1 second)

The full loop:

```
MCP Playground → POST /mcp → MediatR CreateBatchCommand → PostgreSQL INSERT
    → IBatchNotifier.NotifyBatchCreatedAsync → SignalR batchHub
        → Angular SignalRService → Dashboard refetch → UI updates ✅
```

### Testing

The SignalR integration is covered by `Handle_NotifiesSignalRAfterCreate` (uses NSubstitute to verify `IBatchNotifier` is called after successful batch creation).

---

## MCP Server (Model Context Protocol)

TraceLens exposes a **Model Context Protocol** server over HTTP at `/mcp`. AI agents (Claude Desktop, Cursor, VS Code, etc.) can call **3 tools** backed by the same CQRS handlers and PostgreSQL data.

| Tool | Purpose | Parameters |
|---|---|---|
| `get_batches_by_status` | Filter batches by lifecycle status | `status` (string) |
| `get_work_orders` | List all production work orders | — |
| `create_batch` | Create a new batch | `batchNumber`, `workOrderId`, `productId`, `producedQuantity`, `producedQuantityUnit` |

### Connect from real AI clients

**Claude Desktop** (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "tracelens": {
      "url": "http://localhost:5268/mcp"
    }
  }
}
```

**Cursor** (`.cursor/mcp.json`):

```json
{
  "mcpServers": {
    "tracelens": {
      "url": "http://localhost:5268/mcp"
    }
  }
}
```

**VS Code** (`settings.json`):

```json
"mcp.servers": {
  "tracelens": { "url": "http://localhost:5268/mcp" }
}
```

Then ask any of them: *"Show me all batches currently in production"* — the AI will call `get_batches_by_status({ status: "InProduction" })`.

### Connect via MCP Inspector

```bash
npx @modelcontextprotocol/inspector --url http://localhost:5268/mcp
```

### Call a tool (JSON-RPC)

```bash
curl -X POST http://localhost:5268/mcp \
  -H "Content-Type: application/json" \
  -H "Accept: application/json, text/event-stream" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"get_work_orders","arguments":{}}}'
```

### Why this is a real AI integration

- **No duplicate logic** — every tool calls the same `ISender` (MediatR) used by REST endpoints.
- **No fake data** — tools query PostgreSQL via EF Core.
- **Clean Architecture preserved** — MCP is just another entry point, not a new project.
- **Meaningful error messages** — `McpException` surfaces domain and validation errors (e.g. "A batch with the same number already exists") instead of generic SDK wrappers.

### MCP Playground (Angular UI)

TraceLens ships with a built-in **MCP Playground** at `http://localhost:4200/mcp` — a browser-based UI to call all 3 tools live and inspect raw JSON-RPC responses.

- **3 tabs per response** — `Pretty` (structured tables), `Raw JSON` (syntax highlighted), `Meta` (duration, size, timestamp, arguments)
- **Live timing & response size** for every call
- **Copy** and **Download .json** per response
- **Success / Error badges** with cleaned error messages
- **Full end-to-end loop**: Angular → `POST /mcp` (JSON-RPC 2.0 + SSE) → MediatR → EF Core → PostgreSQL → back to UI
- **No external tool needed** — no MCP Inspector, no Postman

---

## Testing — 87 tests passing

```bash
dotnet test
```

| Suite | Tests | Coverage |
|---|---|---|
| **Domain** (Batch, BatchNumber, Quantity, WorkOrder) | 44 | State transitions, guard conditions, ValueObject validation |
| **Application** (Validators, Handlers) | 32 | CQRS commands, validation pipeline, pagination, SignalR verification |
| **API** (Controllers) | 6 | HTTP responses, mediator wiring |
| **Integration** (SQLite) | 4 | OwnsOne round-trip, unique indexes, enum-as-string |
| **Total** | **87** | All green ✅ |

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
`Quantity` has `Value` (decimal) + `Unit` (string). Using OwnsOne, EF Core stores them as two columns (`produced_quantity_value`, `produced_quantity_unit`) in the same table — no extra join, no loss of the unit.

### Why Domain Events?
Decoupling. When a batch passes QC, we can send an email, update a dashboard, or trigger a downstream process — without touching the Batch aggregate.

### Why SignalR?
Users should not have to press refresh to see new batches. SignalR pushes updates over WebSockets, and combined with a `refresh$` Subject + `switchMap`, the Angular dashboard refetches transparently when events arrive — no polling, no stale UI.

### Why OpenIddict (OAuth 2.0) instead of plain JWT?
Plain JWT forces every service to manage signing keys, refresh flows, and revocation. OpenIddict provides a full OAuth 2.0 / OIDC server: password grant, refresh tokens, scope validation, and JWE-encrypted tokens (defense-in-depth against token leakage). The Angular SPA talks to `/connect/token` like any third-party client — no special backend coupling.

### Why MCP Server?
To let AI agents (Claude Desktop, Cursor, VS Code) interact with TraceLens data through a standardized protocol. Tools call the same CQRS handlers as REST endpoints — no duplicate logic, no fake data.

### Why a built-in MCP Playground?
Installing an external MCP client (Inspector, Claude Desktop) creates friction for reviewers. The Angular Playground lets anyone verify the integration in one click, from the browser, with structured tables and syntax-highlighted JSON — proving the MCP server is not just "configured" but actively consumed by the frontend.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 (LTS until Nov 2028) |
| Language | C# 14 |
| API | ASP.NET Core 10, Controllers |
| OpenAPI | Microsoft.AspNetCore.OpenApi 10 + Swashbuckle SwaggerUI |
| MCP | ModelContextProtocol.AspNetCore 2.x |
| Auth | OpenIddict 7.x (OAuth 2.0 / OIDC) |
| Real-time | SignalR (WebSockets) |
| CQRS | MediatR 14 |
| Validation | FluentValidation 12 |
| ORM | EF Core 10 |
| Database | PostgreSQL 16 (Alpine) |
| Logging | Serilog |
| Frontend | Angular 20 + Tailwind CSS v4 + Inter + JetBrains Mono |
| Design System | Fresh Teal (Custom theme, mobile-first) |
| Frontend Auth | JWT bearer interceptor (functional HttpInterceptorFn) |
| Frontend Real-time | @microsoft/signalr 10.x |
| Tests | xUnit, FluentAssertions, EF Core InMemory, SQLite, NSubstitute |
| Container | Docker Compose |

---

## Roadmap

- [x] Clean Architecture — 4 layers, DI, testable
- [x] CQRS with MediatR — 10 features (Batches + Work Orders)
- [x] EF Core + PostgreSQL — migrations, OwnsOne, enums as strings
- [x] Domain Model — aggregates, value objects, domain events
- [x] REST API — controllers, OpenAPI, Swagger UI
- [x] Global Exception Middleware — typed error responses
- [x] Docker Compose — PostgreSQL container, port isolation
- [x] Angular 20 frontend — Dashboard (real API stats), Batches, Create, Details, Work Orders, Quarantine modal, MCP Playground
- [x] MCP Server — 3 tools for AI agents
- [x] MCP Playground — live browser UI to test all 3 tools
- [x] SignalR — real-time batch notifications (hub + notifier + Angular subscriptions)
- [x] OAuth 2.0 with OpenIddict — password grant + refresh tokens, JWE-encrypted tokens
- [x] 87 automated tests — Domain (incl. WorkOrder), Application, API, Integration
- [ ] Playwright E2E tests — full user journey
- [ ] Hangfire — QC reminders, stale batch alerts
- [ ] Redis — caching for dashboard aggregations
- [ ] Playwright E2E tests — full user journey

---

## License

**MIT** © 2026 Mohammad Al-Shahrour

Built with C# / ASP.NET Core, Angular, PostgreSQL — and an MCP server for AI agents.
