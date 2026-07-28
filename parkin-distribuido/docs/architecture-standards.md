# Contexto de estándares del proyecto distribuido

Este documento define el criterio de revisión para `parkin-distribuido`. El monolito de producción queda fuera de alcance y no debe recibir cambios.

## Arquitectura hexagonal / Clean Architecture

```text
Parking.Api (adaptador HTTP) -> Application (casos de uso) -> Domain (reglas)
Infrastructure (persistencia e integraciones) -> Application -> Domain
```

- `Domain` no depende de API, Application ni Infrastructure.
- `Application` contiene DTOs, validación, Commands, Queries y Handlers.
- `Infrastructure` implementa persistencia, identidad, caché e integraciones.
- Los controllers son adaptadores delgados: validan transporte, envían solicitudes y traducen resultados HTTP.
- Los DTOs no exponen entidades EF directamente en nuevos endpoints.

## CQRS y MediatR

MediatR está registrado en `Application/DependencyInjection.cs`, con validación y behaviors de rendimiento/caché. Las nuevas funcionalidades deben usar:

- `IRequest<T>` para Commands y Queries.
- `IRequestHandler<TRequest,TResponse>` en Application.
- `ISender` en controllers.
- `IApplicationDbContextFactory` dentro de handlers, nunca DbContext inyectado en UI/API.

El CRUD de `vehicle-types` funciona como slice de referencia bajo `Application/Features/VehicleTypes` (namespace `VehicleTypeConfigurations` para evitar colisión con el enum de dominio).

## SOLID y buenas prácticas

- Inyección por constructor y dependencias abstraídas.
- Métodos asíncronos con `CancellationToken`.
- Validación en el borde y resultados explícitos.
- Separación de responsabilidades: HTTP, caso de uso, dominio y persistencia.
- JWT es el esquema predeterminado del API distribuido; las cookies pertenecen al frontend.
- No se deben agregar referencias a `parking-monolito`.

## Verificación automatizada

```powershell
dotnet test .\parking-api\ParkingApi.slnx -c Debug --no-restore
node .\scripts\crud-validation.cjs
```

Las pruebas actuales verifican claims JWT, dependencias de capas, presencia de handlers MediatR y que el controller de referencia use `ISender`.

## Frontend y móvil

El frontend mantiene el piloto API en `/pages/tickets-api`; las migraciones deben mover cada slice desde servicios locales a clientes HTTP tipados.

La app MAUI usa `ParkingApiClient`, contratos versionados `/api/v1`, `HttpClient` inyectado y configuración de URL por plataforma. Para nuevas pantallas MAUI se aplican layouts simples, estilos centralizados, accesibilidad y bindings compilados según las guías `maui-ui-best-practices` y `maui-theming`.

## Estado actual

La arquitectura está reforzada en el slice de referencia, no certificada aún para todos los controllers existentes: varios endpoints heredados todavía acceden directamente a `IApplicationDbContextFactory`. Deben migrarse verticalmente antes de considerar la API lista para producción.
