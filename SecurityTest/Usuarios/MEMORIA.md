# MEMORIA — Pruebas de Seguridad para Usuario Autocomplete

## Objetivo

Agregar 3 pruebas de seguridad para el endpoint `GET /api/v1/usuario/autocomplete` usando `WebApplicationFactory<Program>` con InMemory Database.

## Archivos modificados

| Archivo | Propósito |
|---|---|
| `SecurityTest/Usuarios/SecurityTests.cs` | +3 tests de seguridad para autocomplete |

## Las 3 pruebas

1. **`Should_Reject_Autocomplete_With_Empty_Texto`** — texto vacío → 400 BadRequest
2. **`Should_Reject_Autocomplete_Without_Token`** — GET sin token → 401 Unauthorized
3. **`Should_Reject_Autocomplete_With_NonAdmin_Token`** — Token `User` → 403 Forbidden

## Resultados

```
SecurityTest (Usuarios):    22/22  ✔ (19 base + 3 autocomplete)
```

## Comandos útiles

```powershell
dotnet test SecurityTest/SecurityTest.csproj -c Release --no-build --filter "FullyQualifiedName~Usuarios"
```
