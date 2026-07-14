# Pruebas Unitarias — Cliente Autocomplete

## Archivos involucrados

| Archivo | Tipo | Tests |
|---|---|---|
| `UnitTest/Clientes/AutocompleteTests.cs` | Tests | 12 |

**Total: 12 tests — 12/12 correctos**

---

## GET /api/v1/cliente/autocomplete (`Autocomplete`) — 12 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Autocomplete_ReturnsMatchingClientes` | Buscar "Juan" entre "Juan Perez", "Juan Ramirez", "Ana" | 2 resultados |
| `Autocomplete_ReturnsEmpty_WhenNoMatch` | Sin coincidencia | Lista vacía |
| `Autocomplete_IsCaseInsensitive` | Buscar "eduardo" encuentra "Eduardo Sanchez" | 1 resultado (case-insensitive) |
| `Autocomplete_RespectsMaxResultados` | maxResultados=3 de 5 posibles | 3 resultados |
| `Autocomplete_DefaultMaxResultados_Is10` | Sin parámetro, 15 registros | 10 resultados (default) |
| `Autocomplete_ReturnsOrderedByName` | Resultados alfabéticos por strNombreCliente | Orden ascendente |
| `Autocomplete_ReturnsIdAndNombreCliente` | Campos del DTO | id y strNombreCliente poblados |
| `Autocomplete_WithEmptyTexto_ReturnsBadRequest` | texto="" | BadRequestObjectResult |
| `Autocomplete_WithWhitespaceTexto_ReturnsBadRequest` | texto="   " | BadRequestObjectResult |
| `Autocomplete_SpecialChars_ReturnsMatch` | "José" con acento | Coincidencia correcta |
| `Autocomplete_MaxResultadosClamped_WhenExceeds50` | maxResultados=100 | Clamp a 10 (default) y retorna 10 |
| `Autocomplete_MaxResultadosClamped_WhenLessThan1` | maxResultados=0 | Clamp a 10 (default) y retorna 1 |

---

## Resultado general

```
dotnet build -c Release                                          → 0 errores
dotnet test UnitTest --filter "FullyQualifiedName~Autocomplete"  → 12/12
```

## Patrón utilizado

1. `DbContextMock.GetDbContext()` — BD InMemory única por test
2. Seed de datos con `context.CliCliente.AddRange` de `TestDataFactory.CreateClientes()`
3. Servicio creado via helper: `new ClienteService(context, dbResilience)`
4. Assert con FluentAssertions (count, orden, tipo de resultado)
5. Validación de clamping en controller probada directamente
6. Sin hasher — Cliente no maneja contraseñas
