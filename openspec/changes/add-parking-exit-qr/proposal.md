## Why

La vista móvil de parqueo actualmente exige digitar la placa para registrar la salida. Un QR en el ticket de entrada permitirá identificar el parqueo de forma rápida desde la cámara del celular y reducir errores de digitación.

## What Changes

- Agregar una referencia QR única al ticket de entrada del parqueo.
- Mostrar el QR en la vista previa HTML del ticket y conservar la información legible en el ticket POS.
- Añadir un botón de escaneo QR en `/pages/parking-mobile`.
- Leer el QR desde la cámara del dispositivo y reutilizar el flujo existente de salida y cálculo de tarifa.
- Validar que el QR corresponda a un parqueo activo antes de registrar la salida.

## Impact

- Affected specs: parking entry/exit, mobile parking workflow.
- Affected code: `TicketData`, `TicketService`, `MobileParking.razor`, JavaScript de QR/cámara y pruebas.
- No se modifica la ruta ni el comportamiento de la página operativa `/pages/tickets` salvo la incorporación del QR en los tickets de entrada.
- No se agregará información sensible al QR; contendrá únicamente un identificador opaco del registro.
