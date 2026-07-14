# Pruebas Unitarias — EstadoVenta (GET)

## Archivos involucrados

| Archivo | Tipo | Tests |
|---|---|---|
| `UnitTest/EstadoVenta/GetTests.cs` | Tests | 8 |

**Total: 8 tests — 8/8 correctos**

---

## GET /api/v1.0/estadoventa (`GetAll`) — 4 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetAll_EmptyDatabase_ReturnsEmptyPagedResult` | BD vacía | Items vacío, TotalCount=0, PageNumber=1, PageSize=20 |
| `GetAll_ReturnsItemsWithDefaultPagination` | 2 estados | TotalCount=2, Items.Count=2 |
| `GetAll_WithCustomPageSize_ReturnsCorrectSize` | 5 estados, pageSize=2 | Items.Count=2, PageSize=2 |
| `GetAll_NullQueryParams_UsesDefaults` | QueryParams=null | PageNumber=1, PageSize=20 |

## GET /api/v1.0/estadoventa/{id} (`GetById`) — 4 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetById_ExistingId_ReturnsDto` | ID existente | OkObjectResult, datos correctos (strValor, strDescripcion) |
| `GetById_NonExistentId_ReturnsNotFound` | ID=999 | NotFoundResult |
| `GetById_NegativeId_ReturnsNotFound` | ID=-1 | NotFoundResult |
| `GetById_ZeroId_ReturnsNotFound` | ID=0 | NotFoundResult |

---

## Resultado general

```
dotnet build -c Release                                               → 0 errores
dotnet test UnitTest --filter "FullyQualifiedName~EstadoVenta"        → 8/8
```

## Patrón utilizado

1. `DbContextMock.GetDbContext()` — BD InMemory única por test
2. Seed de datos con `context.VenCatEstado.Add`
3. Controller creado via helper: `new EstadoVentaController(new VenCatEstadoService(context))`
4. Assert con FluentAssertions
5. Sin hasher ni DbResilience — EstadoVenta es solo lectura (solo GetAll + GetById)
6. Endpoints requieren `[Authorize(Policy = "AdminOnly")]`
