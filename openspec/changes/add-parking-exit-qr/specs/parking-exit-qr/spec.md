## ADDED Requirements

### Requirement: QR de entrada para salida móvil

El sistema SHALL generar una referencia QR protegida para cada entrada de parqueo y SHALL permitir usarla para identificar el registro activo sin digitar la placa.

#### Scenario: Lectura válida desde la vista móvil

- **WHEN** un usuario autenticado escanea el QR de un ticket con un parqueo activo
- **THEN** el sistema identifica el registro, calcula el valor y registra la salida usando el flujo existente

#### Scenario: QR inválido o parqueo cerrado

- **WHEN** el usuario escanea un QR alterado, desconocido o asociado a un parqueo ya cerrado
- **THEN** el sistema rechaza la operación, no modifica datos y permite volver a digitar la placa

#### Scenario: Cámara no disponible

- **WHEN** el navegador no concede permiso, no tiene cámara o no soporta el lector
- **THEN** la vista móvil informa el problema y conserva el input manual como alternativa
