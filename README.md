# TraceLens

> Manufacturing batch traceability — end to end

A full-stack **C# / ASP.NET Core 10 + Angular 20** platform that tracks every production batch — which raw material lots went into it, which quality checks it passed, and which customer received it. Strict state machine, full audit trail, domain events, real-time updates, **OAuth 2.0 authentication**, **async event pipeline** via RabbitMQ, **97 automated tests**, **11 Playwright E2E tests**, background jobs via Hangfire, **Redis caching** (30s TTL), and an **MCP server for AI agents**.

![TraceLens Demo](docs/tracelens-show.gif)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![C#](https://img.shields.io/badge/C%23-14-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![Angular](https://img.shields.io/badge/Angular-20-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.dev)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.13-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)](https://www.rabbitmq.com)
[![MassTransit](https://img.shields.io/badge/MassTransit-8.3-blue?style=for-the-badge)](https://masstransit.io)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://docker.com)
[![MCP](https://img.shields.io/badge/MCP-Model_Context_Protocol-blueviolet?style=for-the-badge)](https://modelcontextprotocol.io)
[![OAuth](https://img.shields.io/badge/OAuth-2.0-blueviolet?style=for-the-badge&logo=openid)](https://openid.net/connect/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-purple?style=for-the-badge)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![Hangfire](https://img.shields.io/badge/Hangfire-background_jobs-orange?style=for-the-badge)](https://www.hangfire.io)
[![Redis](https://img.shields.io/badge/Redis-caching-red?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io)
[![Tests](https://img.shields.io/badge/Tests-97_passing-success?style=for-the-badge)](tests/)
[![E2E](https://img.shields.io/badge/E2E-11_passing-blue?style=for-the-badge&logo=playwright)](frontend/e2e)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

---

## What it does

| Feature | How |
|---|---|
| **Batch lifecycle** | Created → InProduction → QCPending → QCPassed → Shipped |
| **Alternative branches** | QCFailed, Quarantined |
| **Material traceability** | Every batch records the material lots it consumed |
| **Quality control** | Pass/fail with automatic status transition |
| **Audit trail** | Every state change persisted to `audit_logs` (PostgreSQL, `jsonb` payload) |
| **Domain events** | BatchCreated, QualityCheckPassed, BatchQuarantined |
| **Rich domain model** | Invalid transitions throw at the domain level |
| **Clean architecture** | Domain → Application → Infrastructure → API + Worker |
| **CQRS** | MediatR handlers + FluentValidation pipeline |
| **Angular 20 frontend** | Dashboard (real API stats), Batches, Create, Details, Work Orders, MCP Playground, **Activity Stream** |
| **Dashboard KPIs** | Live counts from PostgreSQL — Total, InProduction, QCPassed, Quarantined, QC success rate |
| **SignalR real-time** | Live dashboard + batches updates on every batch event (no refresh) |
| **OAuth 2.0 (OpenIddict)** | Embedded authorization server — password grant + refresh tokens, JWE-encrypted tokens |
| **RabbitMQ + MassTransit** | 2 integration events published by 6 handlers → 2 Worker consumers → audit_logs |
| **Worker (IHostedService)** | TraceLens.Worker consumes events, writes to audit_logs, ready for Slack/email hooks |
| **Activity Stream** | Real-time audit log in Angular — polling × 3 after SignalR to catch async Worker writes |
| **MCP server** | 3 tools for AI agents (Claude Desktop, Cursor, VS Code) |
| **MCP Playground** | Built-in Angular UI to test all 3 MCP tools live from the browser |
| **Background jobs (Hangfire)** | Hourly stale-batch detection + protected dashboard at `/hangfire` |
| **Redis cache** | 30s TTL for batches list queries + invalidation on create — 8x faster second call |
| **Observability** | Serilog + OpenAPI + Swagger UI |
| **E2E tested** | 11 Playwright tests — auth, batches, MCP, SignalR real-time, Activity, Hangfire |

---

## Quick Start

**Prerequisites:** .NET 10 SDK, Node 20+, Docker Desktop 4.x+, Angular CLI 20+.

```powershell
git clone https://github.com/Eng-MohammadAlShahrour/TraceLens-FullStack-CSharp-MCP-AspNetCore-Angular-PostgreSQL-Docker.git
cd TraceLens-FullStack-CSharp-MCP-AspNetCore-Angular-PostgreSQL-Docker

# 1. Start PostgreSQL + Redis + RabbitMQ
docker compose up -d

# 2. Run the API (Terminal 2)
dotnet run --project src/TraceLens.API

# 3. Run the Worker (Terminal 3 — new window)
dotnet run --project src/TraceLens.Worker

# 4. Run the frontend (Terminal 4 — new window)
cd frontend
npm install
npm start
Login
Open http://localhost:4200 → you'll be redirected to /login.

Field	Value
Username	admin
Password	Admin123!
URLs
Service	URL
API	http://localhost:5268
Swagger UI	http://localhost:5268/swagger
Angular	http://localhost:4200
MCP	http://localhost:5268/mcp
OAuth Token Endpoint	http://localhost:5268/connect/token
Hangfire Dashboard	http://localhost:5268/hangfire
RabbitMQ Management UI	http://localhost:15672 (guest / guest)
Containers use host ports 5433 (PostgreSQL), 6379 (Redis), 5672 (RabbitMQ AMQP), and 15672 (RabbitMQ UI) to avoid conflicts with local services.

Architecture
6-Project Solution (Clean Architecture + Worker)
Project	Purpose	Tech
TraceLens.Domain	Aggregates, Value Objects, Enums, Domain Events, AuditLog	Pure C#
TraceLens.Application	CQRS, MediatR, FluentValidation, IEventPublisher, IntegrationEvents	MediatR 14
TraceLens.Infrastructure	ApplicationDbContext, configurations, migrations, seeder, Auth	EF Core 10 + Npgsql
TraceLens.API	Controllers, middleware, OpenAPI, Serilog, MCP tools, SignalR hubs, Hangfire jobs, Publishers	ASP.NET Core 10
TraceLens.Worker	Console + IHostedService, 2 MassTransit Consumers	MassTransit.RabbitMQ 8.3
TraceLens.Tests	97 tests (Domain + Application + API + Integration + Jobs + Worker)	xUnit + FluentAssertions
frontend/	Angular SPA + Playwright E2E	Angular 20 + Tailwind v4
Async Event Pipeline
text
HTTP Request
    │
    ▼
Handler → SaveChanges (DB commit)
    ├─→ SignalR  (in-process, ~5ms)         → UI updates immediately
    └─→ RabbitMQ (MassTransit, durable)     → cross-process, retryable
              │
              ▼
     TraceLens.Worker (IHostedService)
              ├─→ BatchCreatedConsumer       → INSERT audit_logs
              └─→ BatchStatusChangedConsumer → INSERT audit_logs (+ reason for Quarantine)
                       │
                       ▼
              API GET /api/activity → Angular /activity page
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

AuditLog (entity)
├── Id, EventType, AggregateId, AggregateType
├── BatchNumber, PreviousStatus, NewStatus, Reason
├── Payload (jsonb) — full event record
└── CreatedAt + 3 indexes (created_at, aggregate_id, event_type)
API Reference
Batches (/api/batches) — 8 endpoints
All endpoints require Authorization: Bearer <token> (OAuth 2.0 access token).

Method	Path	Description
POST	/api/batches	Create a new batch
GET	/api/batches/{id}	Get batch by ID
GET	/api/batches?page=1&pageSize=20	Paginated list (cached 30s)
POST	/api/batches/{id}/start-production	Created → InProduction
POST	/api/batches/{id}/pass-qc	InProduction → QCPassed
POST	/api/batches/{id}/fail-qc	InProduction → QCFailed
POST	/api/batches/{id}/ship	QCPassed → Shipped
POST	/api/batches/{id}/quarantine	any → Quarantined (requires reason)
Work Orders (/api/work-orders) — 2 endpoints
Method	Path	Description
POST	/api/work-orders	Create a work order
GET	/api/work-orders?page=1&pageSize=20	Paginated list
Activity (/api/activity) — 1 endpoint
Method	Path	Description
GET	/api/activity?limit=100	Recent audit logs (from Worker consumers)
Auth (/connect/token) — OAuth 2.0
See the OAuth 2.0 section below.

OAuth 2.0 (OpenIddict)
TraceLens includes an embedded OAuth 2.0 / OpenID Connect authorization server using OpenIddict 7.x — no external identity provider required.

Token endpoint
POST /connect/token (application/x-www-form-urlencoded)

Grant type	Credentials	Returns
password	username + password + client_id + client_secret	access_token (JWE), token_type: Bearer, expires_in
refresh_token	refresh_token + client_id + client_secret	new access_token
Demo client
Field	Value
client_id	tracelens-spa
client_secret	tracelens-secret
scope	api
Demo user
Field	Value
username	admin
password	Admin123!
role	Admin
Try it
bash
curl -X POST http://localhost:5268/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=admin&password=Admin123!&client_id=tracelens-spa&client_secret=tracelens-secret&scope=api"
Response:

json
{
  "access_token": "eyJhbGciOiJSU0EtT0FFUCIs...",
  "token_type": "Bearer",
  "expires_in": 3599,
  "username": "admin",
  "display_name": "Mohammad Al-Shahrour",
  "role": "Admin"
}
Why this matters
Real OAuth 2.0 server — not JWT-only. Token issuance, validation, and refresh all flow through OpenIddict.

JWE-encrypted tokens — access tokens use RSA-OAEP + A256CBC-HS512, so a leaked token can't be decoded by third parties.

Angular integration — the SPA calls /connect/token directly, stores the JWE, and attaches it via a functional HTTP interceptor.

Clean Architecture preserved — IAuthService lives in Application; JwtAuthService + OpenIddict config live in Infrastructure/API.

OpenIddict is the default authentication scheme — legacy JWT remains as a fallback, so existing callers don't break during migration.

Message Broker (RabbitMQ + MassTransit)
TraceLens publishes integration events to RabbitMQ via MassTransit 8.3.6 — decoupling side effects (audit trail, future Slack/email) from the request path.

Events published (2 records × 6 handlers)
Event	Publisher	Consumer
BatchCreatedIntegrationEvent	CreateBatchCommandHandler	BatchCreatedConsumer
BatchStatusChangedIntegrationEvent	StartProduction / PassQC / FailQC / Ship / Quarantine	BatchStatusChangedConsumer
Key implementation
IEventPublisher abstraction (Application) — handlers don't touch MassTransit.

MassTransitEventPublisher (API) — uses IPublishEndpoint (scoped, handler-safe), not IBus (singleton).

Best-effort publish — try/catch + log warning; a failed publish never rolls back the DB.

Fail-fast consumer — no try/catch; MassTransit retries (5 attempts → dead-letter).

cfg.ConfigureEndpoints(context) — Worker auto-creates queues bound to exchanges.

Verify
powershell
# RabbitMQ Management UI
http://localhost:15672  (guest / guest)

# CLI — 2 exchanges, 2 queues
docker exec tracelens-rabbitmq rabbitmqctl list_exchanges name type | Select-String "TraceLens"
docker exec tracelens-rabbitmq rabbitmqctl list_queues name messages
Expected exchanges:

text
TraceLens.Application.Common.Events:BatchCreatedIntegrationEvent        fanout
TraceLens.Application.Common.Events:BatchStatusChangedIntegrationEvent  fanout
Expected queues:

text
BatchCreated        0
BatchStatusChanged  0
Worker (TraceLens.Worker)
Standalone IHostedService (Console project) — the only process that consumes messages.

powershell
cd D:\alessandro\TraceLens
dotnet run --project src\TraceLens.Worker
Expected:

text
info: MassTransit[0] Configured endpoint BatchCreated, Consumer: ...BatchCreatedConsumer
info: MassTransit[0] Configured endpoint BatchStatusChanged, Consumer: ...BatchStatusChangedConsumer
info: MassTransit[0] Bus started: rabbitmq://localhost/
Real-time updates (SignalR)
Every batch created or status changed — from REST, MCP, or the Angular UI — broadcasts a SignalR event at /hubs/batches.

The Angular frontend:

Auto-reconnects with exponential backoff

Refreshes the dashboard KPIs live — no manual refresh needed

Prepends new batches to the batches list as they arrive

Updates /activity stream — polls × 3 (400ms, 1000ms, 2000ms) after each event to catch async Worker writes

Try it
Open http://localhost:4200/dashboard in one tab

Open http://localhost:4200/mcp in another tab

In the MCP Playground, execute create_batch with any batch number

Watch:

Dashboard updates live (Total Batches increments)

Activity Stream (/activity) appends the audit row within ~2s

The full loop:

text
MCP Playground → POST /mcp → MediatR → PostgreSQL INSERT
    ├─→ IBatchNotifier → SignalR → Dashboard/Activity refetch → UI updates ✅
    └─→ IBus.Publish → RabbitMQ exchange → Queue → Worker → audit_logs ✅
Background jobs (Hangfire)
TraceLens uses Hangfire (with PostgreSQL storage) to run scheduled background jobs. The Hangfire Dashboard is available at http://localhost:5268/hangfire (protected — only accessible from localhost in development).

Jobs
Job	Schedule	Purpose
check-stale-batches	Hourly	Detects batches stuck in InProduction for > 24h and logs a warning via ILogger
How it works
On API startup, RecurringJobManager.AddOrUpdate<CheckStaleBatchesJob> registers the hourly job.

Hangfire stores job metadata in PostgreSQL (schema: hangfire — created automatically).

Every hour, CheckStaleBatchesJob.Run queries batches where status = InProduction AND started_at < now - 24h.

Any stale batch is logged via Serilog ([WRN] STALE BATCH: {BatchNumber} in production since {StartedAt}).

Viewing the dashboard
Open http://localhost:5268/hangfire from localhost. You'll see:

Recurring Jobs → check-stale-batches (Hourly)

Servers → 1 server online (20 workers)

Succeeded / Failed history

Production note: the current filter allows only localhost/127.0.0.1. For production, replace HangfireAuthorizationFilter with a real authentication scheme (e.g., [Authorize] on the dashboard endpoint).

Caching (Redis)
TraceLens uses Redis 7 (Alpine) as a distributed cache for read-heavy queries. The GetBatchesListQueryHandler caches paginated batch lists for 30 seconds (TTL) — subsequent identical requests return from Redis in ~10ms instead of hitting PostgreSQL.

How it works
GetBatchesListQuery → checks IDistributedCache with key batches:list:p{page}:s{size}

HIT → JSON deserialized and returned (~10ms)

MISS → query PostgreSQL → write to Redis with 30s TTL (~100-500ms)

CreateBatchCommandHandler → invalidates common cache keys (s5, s20, s100) after insert — new batches appear immediately

Why 30s TTL + invalidation?
TTL alone = stale data for up to 30s after creating a batch — bad UX

Invalidation alone = still miss for other pages/sizes

Combined = fresh data on writes, fast reads for repeats (dashboard polling, page navigation)

Measured impact
text
First call  (MISS):  511 ms
Second call (HIT):    66 ms  → 7.8x faster
Resilience
If Redis is down, all cache operations are wrapped in try/catch — the API falls through to PostgreSQL without errors. Redis is a cache, not a dependency.

Verify manually
powershell
# Check keys
docker exec tracelens-redis redis-cli KEYS "TraceLens:*"
# → TraceLens:batches:list:p1:s20

# Inspect a value
docker exec tracelens-redis redis-cli GET "TraceLens:batches:list:p1:s20"
Activity Stream (Angular)
The /activity page displays the audit trail in real time — powered by RabbitMQ consumers writing to audit_logs.

Opens at http://localhost:4200/activity

Loads last 100 events via GET /api/activity

Listens to SignalR BatchCreated + BatchStatusChanged

On each event: polls × 3 (400ms, 1000ms, 2000ms) to catch the async Worker write

No page refresh needed — new rows appear automatically

No fake data — every row is a real persisted audit log, produced by a real MassTransit consumer.

MCP Server (Model Context Protocol)
TraceLens exposes a Model Context Protocol server over HTTP at /mcp. AI agents (Claude Desktop, Cursor, VS Code, etc.) can call 3 tools backed by the same CQRS handlers and PostgreSQL data.

Tool	Purpose	Parameters
get_batches_by_status	Filter batches by lifecycle status	status (string)
get_work_orders	List all production work orders	—
create_batch	Create a new batch	batchNumber, workOrderId, productId, producedQuantity, producedQuantityUnit
Connect from real AI clients
Claude Desktop (claude_desktop_config.json):

json
{
  "mcpServers": {
    "tracelens": {
      "url": "http://localhost:5268/mcp"
    }
  }
}
Cursor (.cursor/mcp.json):

json
{
  "mcpServers": {
    "tracelens": {
      "url": "http://localhost:5268/mcp"
    }
  }
}
VS Code (settings.json):

json
"mcp.servers": {
  "tracelens": { "url": "http://localhost:5268/mcp" }
}
Then ask any of them: "Show me all batches currently in production" — the AI will call get_batches_by_status({ status: "InProduction" }).

Connect via MCP Inspector
bash
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

Meaningful error messages — McpException surfaces domain and validation errors (e.g. "A batch with the same number already exists") instead of generic SDK wrappers.

MCP Playground (Angular UI)
TraceLens ships with a built-in MCP Playground at http://localhost:4200/mcp — a browser-based UI to call all 3 tools live and inspect raw JSON-RPC responses.

3 tabs per response — Pretty (structured tables), Raw JSON (syntax highlighted), Meta (duration, size, timestamp, arguments)

Live timing & response size for every call

Copy and Download .json per response

Success / Error badges with cleaned error messages

Full end-to-end loop: Angular → POST /mcp (JSON-RPC 2.0 + SSE) → MediatR → EF Core → PostgreSQL → back to UI

No external tool needed — no MCP Inspector, no Postman

E2E Testing — 11 Playwright tests
bash
cd frontend
npm run test:e2e        # headless
npm run test:e2e:headed # with browser
npm run test:e2e:ui     # interactive UI
Spec	Tests	Coverage
auth.spec.ts	3	Redirect to login, valid OAuth login, invalid credentials error
batches.spec.ts	2	List loads, create batch and see it in list
mcp.spec.ts	2	get_work_orders returns data, create_batch via MCP → visible
signalr.spec.ts	1	New batch via MCP appears on dashboard without refresh (real-time)
hangfire.spec.ts	1	Hangfire dashboard accessible on localhost (protected)
activity.spec.ts	2	Activity page loads, new MCP batch appears automatically
Total	11	All green ✅
Requires PostgreSQL + Redis + RabbitMQ + API + Worker + Angular running. Uses Page Object Model (e2e/pages/LoginPage.ts).

Testing — 97 tests passing
bash
# Stop API + Worker first (file locks)
Get-Process -Name "TraceLens.API","TraceLens.Worker" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 3
dotnet test
Suite	Tests	Coverage
Domain (Batch, BatchNumber, Quantity, WorkOrder)	44	State transitions, guard conditions, ValueObject validation
Application (Validators, Handlers)	35	CQRS commands, validation pipeline, pagination, SignalR verification, Activity query
API (Controllers)	6	HTTP responses, mediator wiring
Integration (SQLite)	4	OwnsOne round-trip, unique indexes, enum-as-string
Jobs (CheckStaleBatches)	2	Stale-batch detection + no-op when clean
Worker (Consumers)	4	BatchCreatedConsumer, BatchStatusChangedConsumer
Total	97	All green ✅
Design Decisions
Why Clean Architecture (6 projects)?
Testability + independence from frameworks. Domain has zero external dependencies except MediatR. Business rules tested without EF Core, PostgreSQL, or ASP.NET Core. Worker is a separate process — same DI as API, single source of truth for persistence.

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

Why SignalR?
Users should not have to press refresh to see new batches. SignalR pushes updates over WebSockets, and combined with a refresh$ Subject + switchMap, the Angular dashboard refetches transparently when events arrive — no polling, no stale UI.

Why OpenIddict (OAuth 2.0) instead of plain JWT?
Plain JWT forces every service to manage signing keys, refresh flows, and revocation. OpenIddict provides a full OAuth 2.0 / OIDC server: password grant, refresh tokens, scope validation, and JWE-encrypted tokens (defense-in-depth against token leakage). The Angular SPA talks to /connect/token like any third-party client — no special backend coupling.

Why Hangfire?
Stale-batch detection is a background task — it should not block HTTP requests, and it must run even when no user is active. Hangfire provides durable scheduling (backed by PostgreSQL), automatic retries, and a monitoring dashboard — all without writing custom cron code.

Why Redis (with TTL + invalidation)?
Batch lists are read-heavy (dashboard polls, page navigation) but change rarely (only on create). A 30s TTL cache covers repeated reads; explicit invalidation on CreateBatchCommand keeps the list fresh immediately after writes. Combined, this gives 5-10x faster response times on reads — without sacrificing write freshness. Cache operations are best-effort (try/catch) so Redis is a performance optimization, never a dependency.

Why RabbitMQ + MassTransit (vs SignalR alone)?
SignalR = ephemeral (in-process WebSocket, lost on restart, tied to connected clients). RabbitMQ = durable (persisted on disk, retryable, dead-letter, independent services consume without coupling to the API process).

SignalR for UI freshness (~5ms).

RabbitMQ for audit trail durability + future async notifications (Slack, email).

Both channels run in parallel after DB commit (dual-channel pattern) — a failure in either does not roll back the database.

Why a separate Worker project?
Background consumers are a different lifecycle than HTTP requests. Running them in the API would:

Couple scaling (API replicas × consumer replicas = duplicate consumption)

Block API resources under load

Make deployment independence impossible

Worker is a Console IHostedService — same Infrastructure DI, own process, own scaling.

Why polling × 3 on /activity?
API SignalR fires before Worker writes to audit_logs (async gap: 0-2000ms). A single poll after 500ms can miss the write. Three retries (400ms, 1000ms, 2000ms) cover 99% of cases without constant polling (which would waste API + DB resources). This is event-triggered polling — the pragmatic solution before adding Worker→SignalR (which would require a second WebSocket client).

Why MCP Server?
To let AI agents (Claude Desktop, Cursor, VS Code) interact with TraceLens data through a standardized protocol. Tools call the same CQRS handlers as REST endpoints — no duplicate logic, no fake data.

Why a built-in MCP Playground?
Installing an external MCP client (Inspector, Claude Desktop) creates friction for reviewers. The Angular Playground lets anyone verify the integration in one click, from the browser, with structured tables and syntax-highlighted JSON — proving the MCP server is not just "configured" but actively consumed by the frontend.

Tech Stack
Layer	Technology
Runtime	.NET 10 (LTS until Nov 2028)
Language	C# 14
API	ASP.NET Core 10, Controllers
Worker	.NET 10 Console + IHostedService + MassTransit
OpenAPI	Microsoft.AspNetCore.OpenApi 10 + Swashbuckle SwaggerUI
MCP	ModelContextProtocol.AspNetCore 2.x
Auth	OpenIddict 7.x (OAuth 2.0 / OIDC)
Real-time	SignalR (WebSockets)
Background jobs	Hangfire 1.8 (PostgreSQL storage)
Cache	Redis 7 (StackExchange.Redis 2.x)
Message Broker	RabbitMQ 3.13 (Alpine) + MassTransit.RabbitMQ 8.3.6
CQRS	MediatR 14
Validation	FluentValidation 12
ORM	EF Core 10
Database	PostgreSQL 16 (Alpine)
Logging	Serilog
Frontend	Angular 20 + Tailwind CSS v4 + Inter + JetBrains Mono
Design System	Fresh Teal (Custom theme, mobile-first)
Frontend Auth	JWT bearer interceptor (functional HttpInterceptorFn)
Frontend Real-time	@microsoft/signalr 10.x
Tests	xUnit, FluentAssertions, EF Core InMemory, SQLite, NSubstitute
E2E Tests	Playwright 1.x (Chromium) — 11 tests
Container	Docker Compose
Roadmap
☑ Clean Architecture — 4 layers + Worker, DI, testable
☑ CQRS with MediatR — 11 features (Batches + Work Orders + Activity)
☑ EF Core + PostgreSQL — migrations, OwnsOne, enums as strings
☑ Domain Model — aggregates, value objects, domain events
☑ REST API — controllers, OpenAPI, Swagger UI
☑ Global Exception Middleware — typed error responses
☑ Docker Compose — PostgreSQL + Redis + RabbitMQ containers, port isolation
☑ Angular 20 frontend — Dashboard, Batches, Create, Details, Work Orders, Quarantine modal, MCP Playground, Activity Stream
☑ MCP Server — 3 tools for AI agents
☑ MCP Playground — live browser UI to test all 3 tools
☑ SignalR — real-time batch notifications (hub + notifier + Angular subscriptions)
☑ OAuth 2.0 with OpenIddict — password grant + refresh tokens, JWE-encrypted tokens
☑ Hangfire — hourly stale-batch detection + protected dashboard
☑ Redis — 30s TTL cache for batches list + invalidation on create
☑ RabbitMQ + MassTransit — 2 integration events × 6 publishers × 2 consumers
☑ TraceLens.Worker — Console + IHostedService, audit_logs persistence
☑ Activity Stream UI — real-time audit log in Angular
☑ Playwright E2E tests — 11 tests (auth, batches, mcp, signalr, hangfire, activity)
☑ 97 automated tests — Domain (incl. WorkOrder), Application, API, Integration, Jobs, Worker
License
MIT © 2026 Mohammad Al-Shahrour

Built with C# / ASP.NET Core, Angular, PostgreSQL, Redis, RabbitMQ — and an MCP server for AI agents.
