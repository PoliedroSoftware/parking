## Why

The monolithic parking application is the current production system and must remain stable. At the same time, the repository needs independently deployable API, web frontend, and mobile applications that can evolve without coupling releases to the monolith.

## What Changes

- Preserve `parking-monolito` as the production baseline with no source changes.
- Replace the current `parking-frontend` implementation with a frontend based on the monolith's feature coverage and UI behavior.
- Expand `parking-api` until it provides the backend contracts required by the web and mobile clients.
- Remove project references from `parking-api` to monolith projects and establish an independent backend boundary.
- Adapt `parking-movile` to the same versioned API and complete the agreed functional parity scope.
- Add a capability-parity matrix, shared API contracts, migration checks, and independent build/deployment documentation.

## Impact

- Affected systems: `parking-api`, `parking-frontend`, `parking-movile`, and their deployment configuration.
- Protected system: `parking-monolito`; it remains the production fallback and is not modified by this change.
- Data and security: authentication, tenant isolation, permissions, audit behavior, printing, reporting, and database migrations require explicit parity validation.
- Delivery: this is a staged migration. No production cutover occurs until the decoupled clients and API pass functional, integration, and rollback validation.
