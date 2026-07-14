# Pruebas Unitarias — VentaDetalle (GET, Insert, Autocomplete)

## Archivos involucrados

| Archivo | Tipo | Tests |
|---|---|---|
| `UnitTest/VentaDetalle/GetTests.cs` | Tests | 11 |
| `UnitTest/VentaDetalle/InsertTests.cs` | Tests | 10 |
| `UnitTest/VentaDetalle/AutocompleteTests.cs` | Tests | 12 |

**Total: 33 tests — 33/33 correctos**

---

## GET /api/v1/ventadetalle (`GetAll`) — 7 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetAll_ReturnsEmptyPagedResult_WhenNoDetalles` | BD vacía | Items vacío, TotalCount=0, PageNumber=1, PageSize=20 |
| `GetAll_ReturnsPagedResult_WithDefaultPagination` | 5 detalles | TotalCount=5, Items.Count=5, PageNumber=1 |
| `GetAll_ReturnsCorrectPageSize` | 15 detalles, pageSize=5 | Items.Count=5, PageSize=5 |
| `GetAll_ReturnsCorrectPageNumber` | 25 detalles, page=2, size=10 | Items=11-20, PageNumber=2 |
| `GetAll_ReturnsRemainingItems_OnLastPage` | 25 detalles, page=3, size=10 | Items=5 restantes |
| `GetAll_ReturnsEmpty_WhenPageExceedsTotal` | 1 detalle, page=10 | Items vacío |
| `GetAll_WithNullQueryParams_UsesDefaults` | QueryParams=null | PageNumber=1, PageSize=20 |

## GET /api/v1/ventadetalle/{id} (`GetById`) — 4 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetById_ReturnsDetalle_WhenExists` | ID existente | OkObjectResult, campos correctos (idVenVenta, idProProducto, intPiezaVenta, decTotalVenta, strNombreProducto) |
| `GetById_ReturnsNotFound_WhenNotExists` | ID=999 | NotFoundResult |
| `GetById_ReturnsNotFound_WithNegativeId` | ID=-1 | NotFoundResult |
| `GetById_ReturnsNotFound_WithZeroId` | ID=0 | NotFoundResult |
| `GetById_ReturnsCorrectDetalle_WhenMultipleDetallesExist` | 5 detalles, busca intPiezaVenta=3 | Retorna el detalle correcto |

## POST /api/v1/ventadetalle (`Create`) — 10 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Create_ReturnsCreatedAtActionResult` | Create exitoso | CreatedAtActionResult |
| `Create_ReturnsCreatedWithCorrectRouteName` | Ruta correcta | ActionName == "Get" |
| `Create_ReturnsCreatedWithCorrectRouteValues` | Route values | id > 0 en route values |
| `Create_ReturnsDto_WithIdGreaterThanZero` | ID auto-generado | id > 0 |
| `Create_CalculatesDecTotalVentaCorrectly` | Cálculo automático | decTotalVenta = intPiezaVenta * decPrecio |
| `Create_ReturnsDto_WithProductoName` | Navegación FK | strNombreProducto resuelto desde ProProducto |
| `Create_PersistsDetalleInDatabase` | Persistencia | BD: 1 detalle con datos correctos |
| `Create_WithNonExistentVenta_ReturnsBadRequest` | VenVenta FK inválido | 400 "La venta especificada no existe." |
| `Create_WithNonExistentProducto_ReturnsBadRequest` | ProProducto FK inválido | 400 "El producto especificado no existe." |

## GET /api/v1/ventadetalle/buscarproducto (`BuscarProducto`) — 12 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `BuscarProducto_ReturnsMatchingProductos` | Búsqueda parcial por nombre | 2 de 3 coincidencias |
| `BuscarProducto_ReturnsEmpty_WhenNoMatch` | Sin match | Items vacío |
| `BuscarProducto_IsCaseInsensitive` | Mayúsculas/minúsculas | Encuentra sin importar casing |
| `BuscarProducto_ReturnsFormattedText` | Formato correcto | `"Laptop HP \| #: 10 \| $: 15000.00"` |
| `BuscarProducto_RespectsMaxResultados` | maxResultados=5 | 5 resultados |
| `BuscarProducto_DefaultMaxResultados_Is10` | Sin parámetro | 10 resultados default |
| `BuscarProducto_ReturnsOrderedByName` | Orden alfabético | Ordenado por strNombreProducto asc |
| `BuscarProducto_ReturnsIdAndFormattedText` | id + texto | id > 0 + string formateado |
| `BuscarProducto_WithEmptyTexto_ReturnsBadRequest` | texto="" | BadRequest |
| `BuscarProducto_WithWhitespaceTexto_ReturnsBadRequest` | texto="   " | BadRequest |
| `BuscarProducto_MaxResultadosClamped_WhenExceeds50` | max=100 | Clamp a 10 (valor default por rango inválido) |
| `BuscarProducto_MaxResultadosClamped_WhenLessThan1` | max=0 | Clamp a 10 |

---

## Resultado general

```
dotnet build -c Release                                                → 0 errores
dotnet test UnitTest --filter "FullyQualifiedName~VentaDetalle"        → 33/33
```

## Patrón utilizado

1. `DbContextMock.GetDbContext()` — BD InMemory única por test con `TestDbContext`
2. Seed de FKs: CliCliente + SegUsuario + VenCatEstado → VenVenta → ProProducto antes de insertar detalles
3. Service creado via helper: `new VentaDetalleService(context, dbResilience)`
4. Assert con FluentAssertions (.Should().BeOfType\<T\>(), validación de propiedades del DTO y DB)
5. Cálculo de `decTotalVenta = intPiezaVenta * decPrecio` se valida contra el precio del producto seedeado
6. Para autocomplete: seed directo de ProProducto sin depender de otros FKs
