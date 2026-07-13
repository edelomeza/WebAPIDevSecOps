# MEMORIA — Pruebas de Seguridad para TipoEmpleado

## Objetivo

Crear 5 pruebas de seguridad (E2E vía API) para el endpoint `/api/v1/tipoempleado` usando `WebApplicationFactory<Program>` con InMemory Database. Es un endpoint de solo lectura (GetAll + GetById).

## Archivos creados

| Archivo | Propósito |
|---|---|
| `SecurityTest/TipoEmpleado/SecurityTests.cs` | 5 tests de seguridad |
| `UnitTest/Common/TokenHelper.cs` | Generación de JWTs |

## Las 5 pruebas

### Autenticación y Autorización (3)
1. **`Should_Reject_Request_Without_Token`** — GET sin token → 401
2. **`Should_Reject_Token_With_Wrong_Role`** — Token `User` → 403
3. **`Should_Reject_Expired_Token`** — Token expirado → 401

### Headers (2)
4. **`Should_Contain_Security_Headers`** — Headers de seguridad presentes
5. **`Should_Not_Cache_Authenticated_Responses`** — `Cache-Control: no-store`

## Resultados

```
UnitTest (TipoEmpleado):       8/8  ✔
IntegrationTest (TipoEmpleado): 8/8  ✔
SecurityTest (TipoEmpleado):   5/5  ✔
Total:                        21/21  ✔
```

## Comandos útiles

```powershell
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~TipoEmpleado"
dotnet test UnitTest/UnitTest.csproj --no-build --filter "FullyQualifiedName~TipoEmpleado"
dotnet test IntegrationTest/IntegrationTest.csproj --no-build --filter "FullyQualifiedName~TipoEmpleado"
```
