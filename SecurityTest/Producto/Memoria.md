# MEMORIA — Pruebas de Seguridad para Producto

## Objetivo

Crear 21 pruebas de seguridad (E2E vía API) para el endpoint `/api/v1/producto` usando `WebApplicationFactory<Program>` con InMemory Database, verificando que los controles de seguridad funcionan correctamente.

## Archivos creados

| Archivo | Propósito |
|---|---|
| `SecurityTest/Producto/SecurityTests.cs` | 21 tests de seguridad |
| `UnitTest/Common/TokenHelper.cs` | Generación de JWTs (válido, expirado, por rol) |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs (Create/Update/Delete) |

## Las 21 pruebas

### Autenticación y Autorización (6)
1. **`Should_Reject_Request_Without_Token`** — GET/POST/PUT/DELETE sin token → 401
2. **`Should_Reject_Token_With_Wrong_Role`** — Token con rol `User` en endpoint admin → 403
3. **`Should_Reject_Expired_Token`** — Token expirado → 401
4. **`Should_Reject_Blacklisted_Token`** — Token revocado → 401
5. **`Should_Reject_Tampered_Token`** — Token con payload alterado → 401
6. **`Should_Reject_Invalid_Signature`** — Token firmado con otra key → 401

### Validación de Entrada (5)
7. **`Should_Reject_SQL_Injection_In_Nombre`** — `'; DROP TABLE ProProducto; --` → 400
8. **`Should_Reject_XSS_In_Nombre`** — `<script>alert('xss')</script>` → 400
9. **`Should_Reject_Empty_Nombre`** — String vacío → 400
10. **`Should_Reject_Overly_Long_Nombre`** — 51 caracteres → 400
11. **`Should_Reject_Negative_Existencia`** — `intNumeroExistencia = -1` → 400

### Lógica de Negocio (6)
12. **`Should_Reject_Negative_Id`** — GET `/api/v1/producto/-1` → 404
13. **`Should_Reject_Stale_RowVersion`** — PUT con `RowVersion` antigua → 409
14. **`Should_Reject_NonExistent_Producto`** — GET `/api/v1/producto/9999` → 404
15. **`Should_Reject_Search_Without_Texto`** — GET `/buscar` sin parámetro texto → 400
16. **`Should_Reject_Search_With_Empty_Texto`** — GET `/buscar?texto=` → 400
17. **`Should_Reject_Zero_Precio`** — `decPrecio = 0` → 400

### Headers y Cache (3)
18. **`Should_Contain_Security_Headers`** — GET `/api/v1/producto` verifica headers de seguridad
19. **`Should_Not_Cache_Authenticated_Responses`** — GET autenticado tiene `Cache-Control: no-store`
20. **`Should_Reject_Search_With_NonAdmin_Token`** — Token `User` en endpoint de búsqueda → 403

### Precio inválido (1)
21. **`Should_Reject_Zero_Precio`** — `decPrecio = 0` → 400

## Problemas detectados

### Nombres duplicados permitidos

Producto no tiene campo único — `strNombreProducto` no tiene índice único en BD ni validación de unicidad en servicio. El test de duplicado (#14 en Cliente) no aplica; en su lugar se verifica que `AllowsDuplicateNombre` retorna 201.

## Resultados

```
UnitTest (Producto):    46/46  ✔
IntegrationTest (Producto): 38/38  ✔
SecurityTest (Producto):  21/21  ✔
Total:                  105/105  ✔
```

## Comandos útiles

```powershell
# Ejecutar solo tests de seguridad de Producto
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~Producto"

# Ejecutar todos los tests del módulo Producto
dotnet test UnitTest/UnitTest.csproj --no-build --filter "FullyQualifiedName~Producto"
dotnet test IntegrationTest/IntegrationTest.csproj --no-build --filter "FullyQualifiedName~Producto"
dotnet test SecurityTest/SecurityTest.csproj --no-build --filter "FullyQualifiedName~Producto"
```
