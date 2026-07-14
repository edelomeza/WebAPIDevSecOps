# MEMORIA — Pruebas de Seguridad para Venta

## Objetivo

Crear 12 pruebas de seguridad (E2E vía API) para el endpoint `/api/v1/venta` usando `WebApplicationFactory<Program>` con InMemory Database. El módulo Venta es CRUD completo con endpoints GetAll, GetById, Create y Search.

## Archivos creados

| Archivo | Propósito |
|---|---|
| `SecurityTest/Venta/SecurityTests.cs` | 12 tests de seguridad |

## Las 12 pruebas

### Autenticación y Autorización (6)
1. **`Should_Reject_Request_Without_Token`** — GET, GET by id, POST sin token → 401
2. **`Should_Reject_Token_With_Wrong_Role`** — Token `User` → 403
3. **`Should_Reject_Expired_Token`** — Token expirado → 401
4. **`Should_Reject_Blacklisted_Token`** — Token en blacklist → 401
5. **`Should_Reject_Tampered_Token`** — Token alterado → 401
6. **`Should_Reject_Invalid_Signature`** — Firma inválida → 401

### Validación de IDs (3)
7. **`Should_Reject_Negative_Id`** — GET con id=-1 → 404
8. **`Should_Reject_NonExistent_Venta`** — GET con id=9999 → 404
9. **`Should_Reject_Create_With_Empty_IdCliCliente`** — POST con idCliCliente=0 → 400
10. **`Should_Reject_Create_With_Empty_IdSegUsuario`** — POST con idSegUsuario=0 → 400

### Headers (2)
11. **`Should_Contain_Security_Headers`** — Headers de seguridad presentes
12. **`Should_Not_Cache_Authenticated_Responses`** — `Cache-Control: no-store`

## Resultados

```
UnitTest (Venta):          33/33  ✔
IntegrationTest (Venta):   19/19  ✔
SecurityTest (Venta):      12/12  ✔
Total:                     64/64  ✔
```

## Comandos útiles

```powershell
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~Venta"
dotnet test UnitTest/UnitTest.csproj --no-build --filter "FullyQualifiedName~Venta"
dotnet test IntegrationTest/IntegrationTest.csproj --no-build --filter "FullyQualifiedName~Venta"
```
