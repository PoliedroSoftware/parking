<div align="center">

# Poliedro Parking — Blazor Server

Clean Architecture, CQRS-driven parking management platform built with .NET 9 and Blazor Server.

</div>

## Overview

Ace Parking is a multi-tenant parking management system. It provides administration and operations for car parks, gates, zones, space groups, memberships, vehicles, charging rules, holidays, auditing, and system logs — with real-time updates, background jobs, localization, and strong observability.

## Key Features

- Multi-tenant domain model (Tenants, Tenant Users)
- Entities: Carparks, Zones, Gates, SpaceGroups, Members, Vehicles, Charges, Holidays
- Memberships and rentals with vehicle assignments
- Real-time UI updates via SignalR
- Background processing with Hangfire (dashboard at `/jobs`)
- Robust logging with Serilog (file/console + optional DB/Seq sinks)
- CQRS with MediatR, validation with FluentValidation
- Caching with FusionCache
- Localization with resource files (cookie-based culture selection)
- PDF generation (QuestPDF) and Excel exports (ClosedXML)
- Image processing (ImageSharp)

## Architecture

The project follows Clean Architecture and vertical feature slices:

- `Domain` — entities, value objects, domain events, enums
- `Application` — CQRS handlers, validators, pipeline behaviors, feature services
- `Infrastructure` — EF Core, Identity, persistence, integrations, logging
- `Server.UI` — Blazor Server UI, DI wiring, middleware, endpoints

Cross-cutting patterns include:

- CQRS via MediatR with behaviors for validation, performance, caching, and cache invalidation
- EF Core with provider options: SQL Server, PostgreSQL, SQLite
- ASP.NET Core Identity with Data Protection stored in the database
- Serilog logging with enrichers (user info, IP, agent) and rolling file logs

## Tech Stack

- .NET 9 / ASP.NET Core Blazor Server (C#)
- Entity Framework Core 9 (SQL Server, PostgreSQL, SQLite)
- MediatR, FluentValidation, Ardalis.Specification
- FusionCache, AutoMapper
- Serilog (file, console, MSSQL, PostgreSQL, SQLite, Seq)
- Hangfire (in-memory by default)
- SignalR, MudBlazor UI
- QuestPDF, ImageSharp
- MailKit/MimeKit (SMTP), Minio SDK (object storage)

## Project Structure

```
openspec/                # Project context and specs (OpenSpec)
src/
  Domain/               # Core domain model
  Application/          # CQRS, validation, behaviors, services
  Infrastructure/       # EF Core, Identity, logging, integrations
  Migrators/            # Provider-specific EF Core migrations
  Server.UI/            # Blazor Server front-end + host
.github/workflows/      # CI workflows
Dockerfile              # Multi-stage build & runtime image
```

## Getting Started

### Prerequisites

- .NET SDK 9.0+
- One of: SQL Server, PostgreSQL, or SQLite (for persistence), or use in-memory for development

### Configure

Configuration is typically provided via `appsettings.json` and/or environment variables.

- Database settings (required for persistent DB):
  - `DatabaseSettings:DBProvider` — one of `SqlServer`, `Npgsql`, `SqLite`
  - `DatabaseSettings:ConnectionString` — connection string for the selected provider
- Optional dev flag: `UseInMemoryDatabase` (if true, skips DB sinks for Serilog)
- Logging sinks are auto-configured via Serilog and environment; file logs are written under `src/Server.UI/log`.
- Example environment files: `.env.example`, `sample.env`

### Build & Run (Local)

Option 1 — run the UI project directly:

```
dotnet restore AceParking.Blazor.slnx
dotnet build AceParking.Blazor.slnx -c Debug
dotnet run -p src/Server.UI
```

The app serves on HTTP/HTTPS per Kestrel configuration. Hangfire dashboard is available at `/jobs` (authorized access only).

### Database Migrations

Provider-specific migrator projects are included:

- `src/Migrators/Migrators.MSSQL`
- `src/Migrators/Migrators.PostgreSQL`
- `src/Migrators/Migrators.SqLite`

Typical workflow:

1) Ensure `DatabaseSettings` is configured for your provider.
2) From the corresponding migrator directory, use EF Core CLI to create/apply migrations (examples):

```
# From src/Migrators/Migrators.MSSQL (similar for others)
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Note: If EF tooling needs a startup project, specify `-s ../..//Server.UI` to use the application host.

## Docker

The provided `Dockerfile` builds and publishes the Blazor Server app and generates a self-signed certificate for TLS.

Build and run:

```
docker build -t ace-parking .
docker run -p 8080:80 -p 8443:443 --name ace-parking ace-parking
```

Environment variables used by the container (defaults are set in the image):

- `ASPNETCORE_URLS` — default `http://+:80;https://+:443`
- `ASPNETCORE_Kestrel__Certificates__Default__Path` — `/app/https/aspnetapp.pfx`
- `ASPNETCORE_Kestrel__Certificates__Default__Password` — placeholder password baked in the image (replace for production)

Provide your own certificate and secrets for production deployments.

## Observability & Operations

- Logs: Serilog writes rolling files to `src/Server.UI/log` and console; optional sinks for MSSQL, PostgreSQL, SQLite, Seq
- Jobs: Hangfire dashboard at `/jobs`
- Caching: FusionCache configurable via services
- Localization: Cookie-based culture; Accept-Language header is ignored by middleware on purpose

## CI/CD

GitHub Actions build on push/PR to `main`:

- Workflow: `.github/workflows/dotnet.yml`
- Uses `actions/setup-dotnet@v5` with `9.0.x`, restores and builds `AceParking.Blazor.slnx`

## Contributing

- Follow the existing layering and feature folders (`Application/Features/*`, `Server.UI/Pages/*`)
- Prefer small, vertical changes with tests where relevant (Application handlers/validators, Infrastructure integrations)
- Keep specs in `openspec/` updated when introducing new capabilities or changes

## License

This project is licensed under the MIT License. See `LICENSE` for details.

