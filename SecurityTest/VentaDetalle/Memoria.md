# MEMORIA — Pruebas de Seguridad para VentaDetalle

## Objetivo

Crear 13 pruebas de seguridad (E2E vía API) para el endpoint `/api/v1/ventadetalle` usando `WebApplicationFactory<Program>` con InMemory Database. El módulo VentaDetalle es un controlador con endpoints GetAll, GetById, BuscarProducto (autocomplete) y Create.

## Archivos creados

| Archivo | Propósito |
|---|---|
| `SecurityTest/VentaDetalle/SecurityTests.cs` | 13 tests de seguridad |

## Las 13 pruebas

### Autenticación y Autorización (6)
1. **`Should_Reject_Request_Without_Token`** — GET, GET by id, GET buscarproducto, POST sin token → 401
2. **`Should_Reject_Token_With_Wrong_Role`** — Token `User` → 403
3. **`Should_Reject_Expired_Token`** — Token expirado → 401
4. **`Should_Reject_Blacklisted_Token`** — Token en blacklist → 401
5. **`Should_Reject_Tampered_Token`** — Token alterado → 401
6. **`Should_Reject_Invalid_Signature`** — Firma inválida → 401

### Validación de IDs (3)
7. **`Should_Reject_Negative_Id`** — GET con id=-1 → 404
8. **`Should_Reject_NonExistent_VentaDetalle`** — GET con id=9999 → 404
9. **`Should_Reject_Create_With_Empty_IdVenVenta`** — POST con idVenVenta=0 → 400
10. **`Should_Reject_Create_With_Empty_IdProProducto`** — POST con idProProducto=0 → 400
11. **`Should_Reject_Create_With_Empty_IntPiezaVenta`** — POST con intPiezaVenta=0 → 400

### Headers (2)
12. **`Should_Contain_Security_Headers`** — Headers de seguridad presentes
13. **`Should_Not_Cache_Authenticated_Responses`** — `Cache-Control: no-store`

## Lecciones aprendidas

### Endpoints involucrados en seguridad
VentaDetalle expone 4 endpoints (GetAll, GetById, BuscarProducto, Create), todos protegidos con `[Authorize(Policy = "AdminOnly")]`. El test de "sin token" verifica los 4 simultáneamente para asegurar cobertura completa.

### Validación de modelo vs. lógica de negocio
- `idVenVenta=0`, `idProProducto=0` e `intPiezaVenta=0` son atrapados por `[Range(1, int.MaxValue)]` en el DTO → 400 BadRequest automático.
- IDs inexistentes (9999) pasan la validación de modelo pero son atrapados por la lógica de negocio en el service → 400 con mensaje descriptivo.

## Resultados

```
UnitTest (VentaDetalle):          33/33  ✔
IntegrationTest (VentaDetalle):   18/18  ✔
SecurityTest (VentaDetalle):      13/13  ✔
Total:                            64/64  ✔
```

## Comandos útiles

```powershell
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~VentaDetalle"
dotnet test UnitTest/UnitTest.csproj --no-build --filter "FullyQualifiedName~VentaDetalle"
dotnet test IntegrationTest/IntegrationTest.csproj --no-build --filter "FullyQualifiedName~VentaDetalle"
```
