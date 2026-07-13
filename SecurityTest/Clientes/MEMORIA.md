# MEMORIA — Pruebas de Seguridad para Cliente

## Objetivo

Crear 22 pruebas de seguridad (E2E vía API) para el endpoint `/api/v1/cliente` usando `WebApplicationFactory<Program>` con InMemory Database, verificando que los controles de seguridad funcionan correctamente.

## Archivos creados

| Archivo | Propósito |
|---|---|
| `SecurityTest/Clientes/SecurityTests.cs` | 22 tests de seguridad |
| `UnitTest/Common/TokenHelper.cs` | Generación de JWTs (válido, expirado, por rol) |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs (Create/Update/Delete) |

## Las 22 pruebas

### Autenticación y Autorización (6)
1. **`Should_Reject_Request_Without_Token`** — GET sin token → 401
2. **`Should_Reject_Token_With_Wrong_Role`** — Token con rol `User` en endpoint admin → 403
3. **`Should_Reject_Expired_Token`** — Token expirado → 401
4. **`Should_Reject_Blacklisted_Token`** — Token revocado → 401
5. **`Should_Reject_Tampered_Token`** — Token con payload alterado → 401
6. **`Should_Reject_Invalid_Signature`** — Token firmado con otra key → 401

### Validación de Entrada (7)
7. **`Should_Reject_SQL_Injection_In_Nombre`** — `' OR 1=1 --` → 400
8. **`Should_Reject_XSS_In_Nombre`** — `<script>alert(1)</script>` → 400
9. **`Should_Reject_Empty_Nombre`** — String vacío → 400
10. **`Should_Reject_Overly_Long_Nombre`** — 101 caracteres → 400
11. **`Should_Reject_Telefono_With_Letters`** — Teléfono con letras → 400
12. **`Should_Reject_Telefono_Too_Short`** — Menos de 10 dígitos → 400
13. **`Should_Reject_Telefono_Too_Long`** — Más de 10 dígitos → 400

### Lógica de Negocio (5)
14. **`Should_Reject_Negative_Id`** — GET `/api/v1/cliente/-1` → 404
15. **`Should_Reject_Stale_RowVersion`** — PUT/POST con `RowVersion` antigua → 409
16. **`Should_Reject_Duplicate_Correo`** — POST con mismo `strCorreoElectronico` → 400
17. **`Should_Reject_NonExistent_Cliente`** — GET `/api/v1/cliente/9999` → 404
18. **`Should_Reject_Search_Without_Texto`** — GET `/buscar` sin parámetro texto → 400
19. **`Should_Reject_Search_With_Empty_Texto`** — GET `/buscar?texto=` → 400

### Headers y Cache (2)
20. **`Should_Contain_Security_Headers`** — GET `/health` verifica headers de seguridad
21. **`Should_Not_Cache_Authenticated_Responses`** — GET `/api/v1/cliente` tiene `Cache-Control: no-store`

### Autorización por endpoint (1)
22. **`Should_Reject_Search_With_NonAdmin_Token`** — Token `User` en endpoint de búsqueda → 403

## Problemas detectados

### Nombres con guiones bajos

**Síntoma**: Los helpers de test usaban nombres con guiones bajos (ej. `"conflict_test_cli"`) que eran rechazados por el regex del DTO `^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ ]+$`.

**Solución**: Reemplazar underscores en los nombres únicos de los tests de seguridad.

## Resultados

```
UnitTest (Clientes): 56/56  ✔
IntegrationTest (Clientes): 43/43  ✔
SecurityTest (Clientes):   22/22  ✔
Total:                   121/121  ✔
```

## Comandos útiles

```powershell
# Ejecutar solo tests de seguridad de Clientes
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~Clientes"

# Ejecutar un test específico
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~Should_Reject_Duplicate_Correo"

# Ejecutar todos los tests del módulo Cliente
dotnet test UnitTest/UnitTest.csproj --no-build --filter "FullyQualifiedName~Clientes"
dotnet test IntegrationTest/IntegrationTest.csproj --no-build --filter "FullyQualifiedName~Clientes"
dotnet test SecurityTest/SecurityTest.csproj --no-build --filter "FullyQualifiedName~Clientes"
```
