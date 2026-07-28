## Context

The monolith currently contains the broadest feature set. The existing API exposes only a subset of controllers and its solution references monolith projects. The existing frontend has four main pages and the mobile app has five main views, so copying the monolith is only a starting point; the client layers must be separated from application and persistence concerns.

## Goals / Non-Goals

- Goals: independent API, web, and mobile builds; functional parity for agreed modules; safe incremental rollout; monolith rollback path.
- Non-goals: changing or redeploying the production monolith; changing the production database schema without a reviewed migration; replacing all clients in one release.

## Decisions

- Treat `parking-monolito` as read-only during this migration and protect it with a production tag/branch.
- Use `parking-api` as the sole server boundary for the decoupled clients.
- Version HTTP contracts and keep client DTOs separate from persistence entities.
- Move business rules into API-owned application/domain code or an explicitly versioned shared package; do not retain project references into `parking-monolito` as the final state.
- Migrate by vertical feature slices: authentication, parking operations, tickets/printing, members/vehicles, charges, reports, administration, and mobile workflows.
- Build a parity matrix before removing the current frontend so no existing client-only behavior is lost.
- Use an anti-corruption or compatibility layer where necessary while the API is being completed; remove it once the corresponding API-owned behavior is verified.

## Risks / Trade-offs

- Hidden monolith behavior may be missed: mitigate with endpoint, permission, data, and UI inventories plus acceptance tests.
- Divergent calculations or permissions could affect revenue or tenant isolation: compare results against controlled monolith scenarios before cutover.
- Deleting/replacing the frontend can lose work: tag/archive the current frontend and preserve its useful code before replacement.
- API and client releases may drift: use contract versioning, compatibility windows, and a single source of generated/shared contracts where practical.
- Printing and reports may depend on local infrastructure: validate PrintAgent/thermal printing and export formats separately.

## Migration Plan

1. Baseline production monolith and capture configuration, schema, permissions, and smoke tests.
2. Inventory monolith capabilities and map current API, frontend, and mobile coverage.
3. Define API contracts, authentication, tenant context, error format, pagination, and versioning.
4. Expand API ownership and remove monolith project references.
5. Snapshot the current frontend, then rebuild its structure from the monolith's UI coverage while consuming the API.
6. Migrate mobile workflows to the same contracts and complete parity validation.
7. Run parallel acceptance tests, performance checks, security checks, and rollback rehearsals.
8. Deploy decoupled components progressively while keeping the monolith available as fallback.

## Open Questions

- Which feature set is required for the first decoupled production candidate?
- Should authentication use the existing Identity model, a token service, or a staged compatibility approach?
- Will the API and clients share a package for contracts, or publish contracts through an API specification?
- Should the decoupled applications use the same database during transition or an independently migrated database?
