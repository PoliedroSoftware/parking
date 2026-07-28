## Context

El monolito usa Blazor Server, tickets POS generados como texto ESC/POS y una vista previa HTML. Ya existe una utilidad JavaScript para generar imágenes QR en el navegador.

## Goals / Non-Goals

- Goals: identificar de forma segura un parqueo activo desde un celular y registrar su salida; mantener la digitación como alternativa; evitar cambios destructivos en el flujo actual.
- Non-Goals: crear una API pública para el QR, permitir salida sin autenticación o cambiar el esquema de tarifas.

## Decisions

- El QR transportará un token opaco asociado al `ParkingRecord`, no la placa en texto plano.
- La resolución del token se hará en servidor y verificará que el registro esté activo y pertenezca al contexto autorizado.
- La cámara se integrará mediante JavaScript interop con `getUserMedia` y un lector QR del navegador; la UI ofrecerá fallback a digitación cuando el permiso o la cámara no estén disponibles.
- La vista previa HTML mostrará el QR. El ticket POS conservará el texto y añadirá una instrucción breve; imprimir una imagen QR físicamente requiere confirmar soporte del driver ESC/POS.

## Risks / Trade-offs

- Algunos navegadores exigen HTTPS y permiso explícito para la cámara; se mostrará un error accionable y se mantendrá el input.
- Un token persistente podría reutilizarse; se mitigará verificando estado activo y autorización en cada lectura.

## Migration Plan

No se requiere migración si el token se deriva de un identificador existente y se firma/protege con Data Protection. Probar primero con base en memoria y después con los tres proveedores soportados.
