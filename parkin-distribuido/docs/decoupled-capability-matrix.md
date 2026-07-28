# Decoupled Platform Capability Matrix

This matrix records the current migration baseline. The monolith remains the production source of truth while capabilities marked `pending` are migrated.

| Capability | Monolith | API v1 | Frontend | Mobile | Status |
|---|---|---|---|---|---|
| Authentication | Complete | Login endpoint | Monolith authentication copied | Login flow | Partial |
| Parking entry/exit | Complete | `parking` controller | `/pages/tickets-api` pilot; main UI still local | Entry/exit via API v1 | Partial |
| Active parking and movements | Complete | Supported | API pilot available; main UI still local | Supported | Partial |
| Tickets and printing | Complete | Print/parking endpoints | UI and worker copied | No complete parity | Partial |
| Car washes | Complete | Supported | UI copied, still local | Supported | Partial |
| Members and rentals | Complete | Supported | UI copied, still local | Supported | Partial |
| Vehicle types | Complete | Supported | UI copied, still local | Read-only API client | Partial |
| Reports and arqueo | Complete | Supported | UI copied, still local | Supported | Partial |
| Charges and configuration | Complete | Not exposed yet | UI copied, still local | Not implemented | Pending |
| Gates, zones, spaces and carparks | Complete | Not exposed yet | UI copied, still local | Not implemented | Pending |
| Identity, roles and permissions | Complete | JWT baseline | Still monolith Identity | JWT consumer baseline | Pending |
| Tenancy and audit | Complete | Requires parity verification | Still monolith | Not verified | Pending |

## Current verification

- `parking-api/ParkingApi.slnx`: builds with 0 errors.
- `parking-frontend/PoliedroParking.slnx`: builds with 0 errors as a copied monolith baseline.
- `parking-movile/ParkingMaui.slnx`: Windows target builds with 0 errors.
- `parking-frontend` exposes `/pages/tickets-api` as a non-production API integration pilot.
- `parking-monolito`: no working-tree changes were introduced by this migration.

The frontend is intentionally not marked complete: it currently contains the copied Server UI and local application/infrastructure layers. The API pilot must be validated before replacing the production tickets route, then each feature can move one at a time to API v1.
