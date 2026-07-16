<!-- OPENSPEC:START -->
# OpenSpec Instructions

These instructions are for AI assistants working in this project.

Always open `@/openspec/AGENTS.md` when the request:
- Mentions planning or proposals (words like proposal, spec, change, plan)
- Introduces new capabilities, breaking changes, architecture shifts, or big performance/security work
- Sounds ambiguous and you need the authoritative spec before coding

Use `@/openspec/AGENTS.md` to learn:
- How to create and apply change proposals
- Spec format and conventions
- Project structure and guidelines

Keep this managed block so 'openspec update' can refresh the instructions.

<!-- OPENSPEC:END -->

---

## Build & Run

```
dotnet restore PoliedroParking.slnx
dotnet build PoliedroParking.slnx -c Debug
dotnet run -p src/Server.UI
```

- The solution uses `.slnx` format (not traditional `.sln`). CI confirms: `dotnet restore/build AceParking.Blazor.slnx` (`.github/workflows/dotnet.yml`).
- No test projects exist yet. The `tests/` directory referenced in docs is aspirational; the solution file does not include them.
- The entrypoint DLL (used in Dockerfile) is `CleanArchitecture.Blazor.Server.UI.dll`.

## Namespace vs Repo Name

The root namespace is **`CleanArchitecture.Blazor`**, NOT `AceParking`. The repo name (`aceparking`) differs from the C# namespaces. Use the namespace when writing code; the repo name only matters for Git/file paths.

## Architecture: Layer Dependencies (strict)

```
Server.UI → Application → Domain
Infrastructure → Application → Domain
```

**Hard rules:**
- **NEVER** inject `IApplicationDbContext` or `ApplicationDbContext` directly in UI or Application layers. Use `IApplicationDbContextFactory.CreateAsync(cancellationToken)` to get a scoped context.
- **NEVER** inject `IConfiguration` in UI components.
- **The only place UI can reference Infrastructure is `Program.cs`** (for DI registration).
- DI registration uses three extension methods (must all be called): `AddApplication()` → `AddInfrastructure(config)` → `AddServerUI(config)`.
- `RegisterSerilog()` must be called before the DI setup in `Program.cs`.
- All service interfaces live in `Application/Common/Interfaces/`; implementations in `Infrastructure/Services/`.

For detailed conventions (CQRS templates, naming, anti-patterns), see `.cursorrules` — the authoritative architecture guide for this repo.

## DbContext Access Pattern

```csharp
// ✅ Correct — Application handler
await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
var entity = await db.Entities.FirstOrDefaultAsync(e => e.Id == id, ct);

// ❌ Wrong — never inject DbContext
@inject ApplicationDbContext Context
var x = await Context.Entities.ToListAsync();
```

When using in-memory DB (`UseInMemoryDatabase=true`), the DI setup uses `AddDbContext` (singleton-like). Otherwise, it uses `AddDbContextFactory` — the factory pattern is always the correct path.

## Application Layer: Two Service Patterns

1. **CQRS (MediatR)** — commands/queries/handlers in `Application/Features/<Entity>/` (e.g., `Commands/`, `Queries/`, `EventHandlers/`). These go through MediatR pipeline behaviors: validation → performance → fusion cache → cache invalidation.
2. **Feature Services** — registered in `Application/DependencyInjection.cs` under the same `AddApplication()` call. These are scoped services (e.g., `IChargesService`, `IMemberService`, `ICarparkService`) used for cross-entity business logic. They live alongside CQRS handlers, not in Infrastructure.

## Permissions & Authorization

- Permission constants defined as nested classes: `Application/Common/Security/Permissions.cs` → `Permissions.Products.View`, etc.
- **Policies are auto-generated** at startup by reflecting over `Permissions` nested types (in `Infrastructure/DependencyInjection.cs`).
- Blazor pages use `@attribute [Authorize(Policy = Permissions.Products.View)]`.
- In-component checks: `_accessRights = await PermissionService.GetAccessRightsAsync<CarparksAccessRights>()` then `if (_accessRights.Create) { ... }`.
- When adding a new feature, you must: (1) add permission constants as a `partial class Permissions` in `Application/Features/<Feature>/Security/`, (2) create an `AccessRights` class in the same file.

## Localization

- Cookie-based culture selection; **Accept-Language header is explicitly removed** from the pipeline (`ConfigureServer` in `Server.UI/DependencyInjection.cs`).
- Inject `IStringLocalizer<YourComponent> L` and use `@L["Key"]` in Razor.
- Resource files: `Server.UI/Resources/` and `Application/Resources/`.

## Key Infrastructure Quirks

- **Hangfire**: in-memory storage by default, dashboard at `/jobs` (authorization-filtered).
- **FusionCache**: 120 min default duration, fail-safe enabled (20 min), anti-stampede with jitter. Cache invalidation happens automatically via `CacheInvalidationBehaviour` pipeline.
- **Data Protection**: keys persisted to EF Core DB (via `PersistKeysToDbContext<ApplicationDbContext>`).
- **SignalR**: hub mapped at `ISignalRHub.Url` (`/signalRHub`). Hub filter `UserContextHubFilter` enriches user context.
- **Cookies**: `SameSiteMode.Strict`, `SecurePolicy.Always`, 15-day sliding expiration, `MemoryCacheTicketStore`.
- **Identity**: custom `AuditSignInManager` replaces default `SignInManager`. OAuth providers: Microsoft, Google (Facebook commented out).
- **QuestPDF**: Community license configured in `ConfigureServer()`.
- **PDF/Image processing** in Docker requires Linux native packages: `SkiaSharp.NativeAssets.Linux.NoDependencies` + `HarfBuzzSharp.NativeAssets.Linux` — added during Docker build, not in `.csproj`.

## Formatting & Style

- `.editorconfig` enforces: 4-space indentation, CRLF line endings, UTF-8 BOM, file-scoped namespaces, `var` preferred, `_` prefix on private instance fields.
- ReSharper settings embedded in `.editorconfig` (align multiline, wrap rules, etc.).
- Max line length: 120 characters.

## Migrator Projects

Three provider-specific migration projects exist under `src/Migrators/`. They are **not** entrypoints — they hold EF Core migrations for each DB provider. When adding migrations:
```
dotnet ef migrations add <Name> -s src/Server.UI -p src/Migrators/Migrators.MSSQL
```

## Environment Configuration

- `sample.env` lists all env vars. Key ones: `DB_PROVIDER` (mssql/postgresql/sqlite), `DB_CONNECTION_STRING`, `USE_IN_MEMORY_DATABASE`.
- Development uses `appsettings.json` in `src/Server.UI/`; Docker uses env vars injected via `docker-compose.yml`.
- **Config naming differs**: `appsettings.json` uses nested keys like `DatabaseSettings:DBProvider`, `AppConfigurationSettings:AppName`; env vars use flat names like `DB_PROVIDER`, `AppConfigurationSettings__AppName` (double underscore separator).
- If `UseInMemoryDatabase=true`, everything runs in-memory (no DB required).