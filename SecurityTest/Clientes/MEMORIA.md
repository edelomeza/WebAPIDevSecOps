# MEMORIA — Pruebas de Seguridad para Cliente Autocomplete

## Objetivo

Agregar 5 pruebas de seguridad para el endpoint `GET /api/v1/cliente/autocomplete` usando `WebApplicationFactory<Program>` con InMemory Database.

## Archivos modificados

| Archivo | Propósito |
|---|---|
| `SecurityTest/Clientes/SecurityTests.cs` | +5 tests de seguridad para autocomplete |

## Las 5 pruebas

### Autenticación y Autorización (3)
1. **`Should_Reject_Autocomplete_Without_Token`** — GET sin token → 401
2. **`Should_Reject_Autocomplete_With_Wrong_Role`** — Token `User` → 403
3. **`Should_Reject_Autocomplete_With_Empty_Texto`** — texto vacío → 400

### Response (2)
4. **`Should_Accept_Autocomplete_With_Valid_Token`** — Token Admin → 200 OK
5. **`Autocomplete_Returns_Flat_List`** — Seed cliente, response JSON array plano (no paginado)

## Resultados

```
SecurityTest (Clientes):    17/17  ✔ (12 base + 5 autocomplete)
```

## Comandos útiles

```powershell
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~Clientes"
```
