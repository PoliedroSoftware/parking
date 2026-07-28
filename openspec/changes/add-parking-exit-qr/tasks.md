## 1. Especificación y seguridad

- [x] 1.1 Definir el formato del token QR y su validación con Data Protection.
- [x] 1.2 Documentar escenarios de lectura válida, QR inválido, registro ya cerrado y cámara no disponible.

## 2. Ticket de entrada

- [x] 2.1 Extender los datos del ticket con la referencia QR.
- [x] 2.2 Renderizar el QR en la vista previa HTML de entrada.
- [x] 2.3 Mantener una indicación legible en el ticket POS.

## 3. Vista móvil

- [x] 3.1 Añadir botón y estado de escaneo a `/pages/parking-mobile`.
- [x] 3.2 Integrar cámara mediante JavaScript interop con fallback al input de placa.
- [x] 3.3 Procesar la salida usando la misma lógica de cálculo, persistencia e impresión.

## 4. Verificación

- [ ] 4.1 Añadir pruebas para token válido, token alterado y parqueo no activo.
- [ ] 4.2 Compilar `PoliedroParking.slnx` y probar en HTTPS desde un celular.
