# Pruebas Unitarias — Usuario Autocomplete

## Archivos involucrados

| Archivo | Tipo | Tests |
|---|---|---|
| `UnitTest/Usuarios/QueryTests.cs` | Tests | +6 (sección `GET /autocomplete`) |

**Total: 6 tests — 6/6 correctos** (agregados a los 13 tests existentes de búsqueda)

---

## GET /api/v1/usuario/autocomplete (`Autocomplete`) — 6 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Autocomplete_ReturnsMatchingUsers` | Buscar "ed" entre Eduardo, Edel, Maria, Jose | 2 resultados (Edel, Eduardo) |
| `Autocomplete_RespectsMaxResultados` | maxResultados=2 de 4 posibles | 2 resultados |
| `Autocomplete_ReturnsEmpty_WhenNoMatch` | Buscar "xyz" sin coincidencia | Lista vacía |
| `Autocomplete_IsCaseInsensitive` | Buscar "EDUARDO" entre Eduardo, edel, EDUARDO | 2 resultados |
| `Autocomplete_SpecialChars_ReturnsMatch` | "Jos" encuentra José y Jose | 2 resultados |
| `Autocomplete_ReturnsOrderedByName` | Alpha, Beta, Zeta | Orden alfabético ascendente |

---

## Resultado general

```
dotnet build -c Release                                          → 0 errores
dotnet test UnitTest --filter "FullyQualifiedName~QueryTests"   → 19/19 (13 search + 6 autocomplete)
```

## Patrón utilizado

1. `DbContextMock.GetDbContext()` — BD InMemory única por test
2. Seed de usuarios con `SeedUsers(params string[])` — helper existente en la clase
3. Servicio creado via helper: `new UsuarioService(context, hasherMock.Object, dbResilience)`
4. Assert con FluentAssertions (count, orden, contenido)
5. Reutiliza el `_hasherMock` y `_dbResilience` del constructor de la clase
