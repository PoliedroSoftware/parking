## ADDED Requirements

### Requirement: Production monolith protection

The migration SHALL preserve `parking-monolito` as an unchanged production fallback until the decoupled platform is explicitly approved for cutover.

#### Scenario: Decoupled work is deployed

- **WHEN** API, frontend, or mobile code is changed or deployed
- **THEN** the production monolith source, deployment artifact, and rollback reference remain available and unchanged

### Requirement: Versioned API boundary

The decoupled web and mobile clients SHALL use versioned API contracts for authentication, authorization, tenant context, validation, errors, pagination, and business operations.

#### Scenario: Client calls a supported operation

- **WHEN** a web or mobile client invokes a supported versioned endpoint
- **THEN** the API returns the documented response shape and applies the same tenant, permission, validation, and business rules defined for the approved parity scope

### Requirement: Functional parity tracking

The project SHALL maintain a capability matrix mapping monolith behavior to API endpoints, frontend screens, and mobile workflows before a production cutover is approved.

#### Scenario: A capability is not yet migrated

- **WHEN** a capability is missing or only partially implemented in a decoupled client
- **THEN** it is marked as such in the matrix and the monolith remains the required production path for that capability

### Requirement: Independent project boundaries

The final decoupled API SHALL NOT require project references to `parking-monolito`, and the clients SHALL NOT access database or infrastructure implementations directly.

#### Scenario: API is built independently

- **WHEN** `parking-api` is restored and built from its own solution
- **THEN** it succeeds without source-project references to the monolith and exposes all dependencies required by the approved API scope

### Requirement: Safe staged rollout

The decoupled platform SHALL support non-production validation, progressive deployment, observability, and rollback to the monolith.

#### Scenario: Decoupled release fails validation

- **WHEN** a release fails functional, security, performance, or operational checks
- **THEN** traffic remains or returns to the monolith without requiring a destructive database or source rollback
