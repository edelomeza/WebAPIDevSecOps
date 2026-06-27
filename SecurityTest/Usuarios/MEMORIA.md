# MEMORIA — Pruebas de Seguridad para Usuario

## Objetivo

Crear 15 pruebas de seguridad (E2E vía API) para el endpoint `/api/v1/usuario` usando `WebApplicationFactory<Program>` con InMemory Database, verificando que los controles de seguridad funcionan correctamente.

## Archivos creados

| Archivo | Propósito |
|---|---|
| `SecurityTest/Usuarios/SecurityTests.cs` | 15 tests de seguridad |
| `UnitTest/Common/TokenHelper.cs` | Generación de JWTs (válido, expirado, por rol) |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs (Create/Update/Delete) |

## Las 15 pruebas

### Autenticación y Autorización (6)
1. **`Should_Reject_Unauthenticated_Access`** — GET sin token → 401
2. **`Should_Reject_Token_With_Wrong_Role`** — Token con rol `User` en endpoint admin → 403
3. **`Should_Reject_Expired_Token`** — Token expirado → 401
4. **`Should_Reject_Blacklisted_Token`** — Token revocado → 401
5. **`Should_Reject_Tampered_Token`** — Token con payload alterado → 401
6. **`Should_Reject_Invalid_Signature_Token`** — Token firmado con otra key → 401

### Validación de Entrada (4)
7. **`Should_Reject_SQL_Injection_In_Nombre`** — `' OR 1=1 --` → 400
8. **`Should_Reject_XSS_In_Nombre`** — `<script>alert(1)</script>` → 400
9. **`Should_Reject_Empty_Nombre`** — String vacío → 400
10. **`Should_Reject_Excessively_Long_Nombre`** — 100 caracteres → 400

### Lógica de Negocio (5)
11. **`Should_Reject_Negative_Id`** — GET `/api/v1/usuario/-1` → 400
12. **`Should_Reject_Stale_RowVersion`** — PUT con `RowVersion` antigua → 409
13. **`Should_Reject_Duplicate_Username`** — POST con mismo `strNombre` → 400
14. **`Should_Reject_NonExistent_User`** — GET `/api/v1/usuario/9999` → 404
15. **`Should_Have_Security_Headers`** — GET `/health` verifica headers de seguridad

## Problema resuelto: `RowVersion` con InMemory

**Síntoma**: POST para crear usuario retornaba 500 con:
```
Required properties 'RowVersion' are missing for the instance of entity type 'SegUsuario'
```

**Causa raíz**: EF Core con SQL Server auto-genera columnas `[Timestamp]` (rowversion). Con InMemory no lo hace, y el valor `null` viola la propiedad requerida.

**Solución**: Default `= new byte[] { 1 }` en `SegUsuario.RowVersion` (`WebAPIDevSecOps/Models/SegUsuario.cs:22`). En SQL Server la base de datos sobreescribe el valor; en InMemory evita el error.

**Infraestructura**: Se agregó flag `UseInMemoryDatabase` en `Program.cs`. Tests lo activan vía `builder.UseSetting("UseInMemoryDatabase", "true")` en el factory, lo que hace que `Program.cs` registre `AddDbContext<AppDbContext>(UseInMemoryDatabase)` en lugar de SQL Server.

## Resultados

```
UnitTest:      53/53  ✔ (2s)
IntegrationTest: 4/4  ✔ (6s)
SecurityTest:   22/22 ✔ (6s)
Total:         79/79  ✔
```

## Comandos útiles

```powershell
# Ejecutar solo tests de seguridad
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build

# Ejecutar un test específico
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~Should_Reject_Duplicate_Username"
```
