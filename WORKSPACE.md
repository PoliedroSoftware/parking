# Parking workspace

La raíz contiene únicamente la organización del producto:

```text
parking/
├── parking-monolito/       # sistema actual en producción; no modificar aquí
└── parkin-distribuido/     # API, frontend y aplicación móvil desacoplados
    ├── parking-api/
    ├── parking-frontend/
    └── parking-movile/
```

Abre `Parking.code-workspace` para trabajar con ambos entornos.

- Producción: `parking-monolito/`
- Desarrollo desacoplado: `parkin-distribuido/`
- El API distribuido usa `parking-api/ParkingApi.slnx`.
- El frontend usa `parking-frontend/PoliedroParking.slnx`.
- La app móvil usa `parking-movile/ParkingMaui.slnx`.

Los cambios del distribuido no deben copiarse automáticamente al monolito.
