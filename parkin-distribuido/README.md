# Parking distribuido

Workspace independiente para trabajar el API, frontend y aplicación móvil sin modificar `parking-monolito`.

## Proyectos

- `parking-api/`: API .NET y contratos versionados bajo `/api/v1`.
- `parking-frontend/`: interfaz Blazor derivada del monolito, con piloto en `/pages/tickets-api`.
- `parking-movile/`: aplicación .NET MAUI y cliente compartido.

## Levantar API y frontend

Desde esta carpeta:

```powershell
$env:UseInMemoryDatabase = 'false'
$env:DatabaseSettings__DBProvider = 'postgresql'
$env:DatabaseSettings__ConnectionString = 'Host=localhost;Port=5432;Database=parking_distributed;Username=parking;Password=change-me;'
dotnet run --project .\parking-api\src\Parking.Api --urls http://localhost:5221

$env:ParkingApi__BaseUrl = 'http://localhost:5221'
dotnet run --project .\parking-frontend\src\Server.UI --urls http://localhost:5057
```

## Prueba desplegada aislada

El servidor de prueba `192.168.0.137` mantiene el distribuido separado del monolito:

- API: `http://192.168.0.137:5221`.
- Frontend: `http://192.168.0.137:5057`.
- PostgreSQL: base `parking_distributed`, dentro del compose distribuido; no usa la base de produccion.

El flujo de parqueo soporta entrada, activos, salida por placa y scanner QR en la pantalla principal. El piloto `/pages/tickets-api` verifica el mismo ciclo mediante `/api/v1/parking/entry`, `/active` y `/exit`.

La documentación del API está en `http://localhost:5221/swagger` y el piloto web en `http://localhost:5057/pages/tickets-api`.

## CRUD de verificación

El CRUD de tipos de vehículo está disponible en `/api/v1/vehicle-types`:

```text
GET    /api/v1/vehicle-types
POST   /api/v1/vehicle-types
PUT    /api/v1/vehicle-types/{id}
DELETE /api/v1/vehicle-types/{id}
```

Requiere JWT obtenido desde `POST /api/v1/auth/login`, salvo `GET /api/v1/vehicle-types/active`.

## Arquitectura y pruebas

Las reglas de arquitectura, CQRS, SOLID y el estado de la revisión están en `docs/architecture-standards.md`.

```powershell
dotnet test .\parking-api\ParkingApi.slnx -c Debug --no-restore
node .\scripts\crud-validation.cjs
```
