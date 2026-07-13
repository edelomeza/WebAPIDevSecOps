# MEMORIA — Pruebas de Seguridad para Empleado

## Objetivo

Crear 16 pruebas de seguridad (E2E vía API) para el endpoint `/api/v1/empleado` usando `WebApplicationFactory<Program>` con InMemory Database, verificando que los controles de seguridad funcionan correctamente.

## Archivos creados

| Archivo | Propósito |
|---|---|
| `SecurityTest/Empleado/SecurityTests.cs` | 16 tests de seguridad |
| `UnitTest/Common/TokenHelper.cs` | Generación de JWTs (válido, expirado, por rol) |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs (Create/Update/Delete) |

## Las 16 pruebas

### Autenticación y Autorización (6)
1. **`Should_Reject_Request_Without_Token`** — GET/POST/PUT/DELETE sin token → 401
2. **`Should_Reject_Token_With_Wrong_Role`** — Token con rol `User` → 403
3. **`Should_Reject_Expired_Token`** — Token expirado → 401
4. **`Should_Reject_Blacklisted_Token`** — Token revocado → 401
5. **`Should_Reject_Tampered_Token`** — Token alterado → 401
6. **`Should_Reject_Invalid_Signature`** — Firma inválida → 401

### Validación de Entrada (5)
7. **`Should_Reject_SQL_Injection_In_Nombre`** — `'; DROP TABLE EmpEmpleado; --` → 400
8. **`Should_Reject_XSS_In_Nombre`** — `<script>alert('xss')</script>` → 400
9. **`Should_Reject_Empty_Nombre`** — String vacío → 400
10. **`Should_Reject_Overly_Long_Nombre`** — 51 caracteres → 400
11. **`Should_Reject_Negative_Id`** — GET `/api/v1/empleado/-1` → 404

### Lógica de Negocio (4)
12. **`Should_Reject_Stale_RowVersion`** — PUT con `RowVersion` antigua → 409
13. **`Should_Reject_Duplicate_CURP`** — POST con misma CURP → 400
14. **`Should_Reject_NonExistent_Empleado`** — GET `/api/v1/empleado/9999` → 404

### Headers y Cache (2)
15. **`Should_Contain_Security_Headers`** — GET `/api/v1/empleado` verifica headers
16. **`Should_Not_Cache_Authenticated_Responses`** — `Cache-Control: no-store`

## Problemas detectados

### CURP único con nulls permitidos

El filtro de duplicado usa `strCURP == dto.strCURP && strCURP != null` para permitir múltiples empleados sin CURP. Los tests de seguridad usan CURP únicos predefinidos para evitar colisiones.

## Resultados

```
UnitTest (Empleado):      40/40  ✔
IntegrationTest (Empleado): 22/22  ✔
SecurityTest (Empleado):  16/16  ✔
Total:                    78/78  ✔
```

## Comandos útiles

```powershell
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~Empleado"
dotnet test UnitTest/UnitTest.csproj --no-build --filter "FullyQualifiedName~Empleado"
dotnet test IntegrationTest/IntegrationTest.csproj --no-build --filter "FullyQualifiedName~Empleado"
```
