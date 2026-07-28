## 1. Baseline and protection

- [ ] 1.1 Create a production tag/branch and record the deployed monolith commit.
- [ ] 1.2 Capture production configuration, database provider, migrations, permissions, and operational dependencies.
- [ ] 1.3 Define smoke tests and rollback steps that must remain valid throughout the migration.

## 2. Capability and data parity

- [x] 2.1 Inventory the initial monolith pages, API controllers, frontend pages, mobile views, reports, and printing workflows.
- [x] 2.2 Create the initial capability matrix mapping monolith behavior to API endpoints, frontend screens, and mobile workflows.
- [ ] 2.3 Identify gaps, deprecated behavior, client-only behavior, and high-risk financial/security flows.
- [ ] 2.4 Agree on the first production-candidate parity scope.

## 3. API boundary

- [ ] 3.1 Define versioned contracts, authentication, tenant context, permissions, errors, paging, filtering, and validation rules.
- [ ] 3.2 Implement API-owned application/domain boundaries for the approved feature slices.
- [ ] 3.3 Add integration coverage for calculations, tenant isolation, authorization, tickets, reports, and printing.
- [x] 3.4 Remove project references from `parking-api` to `parking-monolito` for the current API baseline.
- [x] 3.5 Document local API build, run, route, and configuration conventions.

## 4. Web frontend

- [x] 4.1 Snapshot the current `parking-frontend` repository and preserve reusable functionality.
- [ ] 4.2 Replace its implementation with the monolith-derived UI baseline without copying server/database dependencies.
- [x] 4.3 Add the initial typed API client, JWT login, loading states, and error handling for the parking pilot.
- [ ] 4.4 Migrate feature slices and verify behavior against the parity matrix.
- [ ] 4.5 Add independent build, deployment, and environment configuration.

## 5. Mobile application

- [x] 5.1 Audit current MAUI views, services, shared models, and platform-specific behavior.
- [x] 5.2 Migrate the existing mobile API client to versioned API contracts and emulator networking.
- [ ] 5.3 Complete the approved mobile feature scope, including authentication, parking operations, tickets, reports, and failure states.
- [ ] 5.4 Validate Android, iOS, Windows, printing/integration needs, and offline/network recovery behavior.

## 6. Validation and rollout

- [x] 6.0 Install Playwright with local Chromium and automate smoke validation for API v1 and the frontend pilot.
- [ ] 6.1 Run monolith-vs-decoupled acceptance scenarios with equivalent data, beginning with `/pages/tickets-api`.
- [ ] 6.2 Verify security, tenant isolation, audit logs, performance, observability, and backups.
- [ ] 6.3 Test independent deployments and rollback to the monolith.
- [ ] 6.4 Deploy a non-production pilot, then release by feature or client while keeping the monolith unchanged and available.
