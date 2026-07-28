# Parking API

Independent backend baseline for the decoupled web and mobile clients.

## Build and run

```powershell
dotnet restore ParkingApi.slnx
dotnet build ParkingApi.slnx -c Debug
dotnet run --project src/Parking.Api
```

The API targets .NET 9 and owns local copies of `Domain`, `Application`, `Infrastructure`, and provider migrators. It must not reference `parking-monolito`.

## API boundary

Supported routes are versioned under `/api/v1`:

- `/api/v1/auth`
- `/api/v1/parking`
- `/api/v1/carwashes`
- `/api/v1/members`
- `/api/v1/vehicle-types`
- `/api/v1/reports`
- `/api/v1/print`

Swagger is available when the API is running. Configure the database and JWT settings through `src/Parking.Api/appsettings.json` or environment variables; production secrets must not remain in source control.
