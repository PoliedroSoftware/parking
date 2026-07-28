# Project Context

## Purpose
- Multi-tenant parking management platform built on Blazor Server.
- Manage car parks, gates, zones, spaces, members, vehicles, charges, holidays, and audit trails.
- Provide secure identity, localization, real-time updates, background jobs, reporting/exports, and admin tooling.

## Tech Stack
- .NET 9 / ASP.NET Core Blazor Server (C#)
- Entity Framework Core 9 (SQL Server, PostgreSQL, SQLite)
- MediatR (CQRS), FluentValidation, Ardalis.Specification
- ZiggyCreatures.FusionCache for caching
- Serilog (file, console, MSSQL, PostgreSQL, SQLite, Seq)
- Hangfire (jobs, in-memory by default, dashboard at `/jobs`)
- SignalR (real-time), MudBlazor (UI), AutoMapper
- QuestPDF (PDF generation), ImageSharp (image processing)
- MailKit/MimeKit (email), Minio SDK (object storage)

## Project Conventions

### Code Style
- Enforced via `.editorconfig` (C# conventions, analyzers, nullable enabled, implicit usings).
- Target framework: `net9.0`; `LangVersion` default; nullable reference types: enabled.
- No frameworks added without justification; prefer simple, testable constructs.

### Architecture Patterns
- Clean Architecture with layered projects:
  - `Domain` (entities, value objects, domain events, enums)
  - `Application` (CQRS handlers, validators, pipeline behaviors, services)
  - `Infrastructure` (EF Core, Identity, logging, integrations, persistence)
  - `Server.UI` (Blazor Server UI, DI wiring, middleware, endpoints)
- CQRS via MediatR with behaviors: validation, performance, caching, cache invalidation.
- EF Core with provider-specific migrators: `Migrators.MSSQL`, `Migrators.PostgreSQL`, `Migrators.SqLite`.
- Identity with EntityFramework Core and DataProtection in DB.
- Localization via resource files with cookie-based culture; Accept-Language header provider removed.
- Real-time with SignalR, jobs with Hangfire (in-memory default), PDF via QuestPDF.
- Logging with Serilog + enrichers (user info, client IP); rolling file logs under `src/Server.UI/log`.

### Testing Strategy
- No dedicated test projects detected yet; CI builds the solution.
- Recommend: unit tests for Application layer (handlers, validators), integration tests for Infrastructure (DbContext with InMemory/provider containers), bUnit/UI tests for Blazor components where valuable.
- Use EF Core InMemory and provider-specific tests for critical behaviors.

### Git Workflow
- Default branch: `main`.
- GitHub Actions workflow `.github/workflows/dotnet.yml` builds on push/PR to `main` (dotnet 9.0, restore + build `AceParking.Blazor.slnx`).
- Paths under `deploy/**` are ignored by CI.
- Commit message conventions not enforced; Conventional Commits encouraged for clarity.

## Domain Context
- Core entities: Carpark, Zone, Gate, SpaceGroup, Charge, Member, MemberRental, MemberVehicle, Vehicle, Holiday, Tenant, TenantUser, SystemLog, AuditTrail, LoginAudit.
- Multi-tenant awareness is present (Tenant, TenantUser) and should be considered in queries and UI visibility.
- Identity/roles integrated with ASP.NET Core Identity; additional login risk summaries maintained.
- Features are organized by verticals in `Application/Features/*` and UI pages in `Server.UI/Pages/*` with MudBlazor components.

## Important Constraints
- .NET 9 target across projects; Blazor Server runtime.
- Database provider selectable (SQL Server, PostgreSQL, SQLite). In-memory database may be used for dev.
- Hangfire dashboard exposed at `/jobs` with custom authorization filters.
- QuestPDF Community license configured.
- TLS certificate generated in Docker image; password/path set via env vars (placeholder values in Dockerfile).
- Accept-Language header is ignored; localization determined by cookie or default.

## External Dependencies
- Databases: SQL Server, PostgreSQL, SQLite (via EF Core)
- Authentication providers: Google, Microsoft Account (configurable)
- Logging sinks: SQL Server, PostgreSQL, SQLite, Seq, file, console
- Object storage: Minio (optional)
- Email: SMTP via MailKit/MimeKit
