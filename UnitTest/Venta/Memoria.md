# Pruebas Unitarias — Venta (GET, Insert, Search)

## Archivos involucrados

| Archivo | Tipo | Tests |
|---|---|---|
| `UnitTest/Venta/GetTests.cs` | Tests | 13 |
| `UnitTest/Venta/InsertTests.cs` | Tests | 11 |
| `UnitTest/Venta/QueryTests.cs` | Tests | 9 |

**Total: 33 tests — 33/33 correctos**

---

## GET /api/v1/venta (`GetAll`) — 7 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetAll_ReturnsEmptyPagedResult_WhenNoVentas` | BD vacía | Items vacío, TotalCount=0, PageNumber=1, PageSize=20 |
| `GetAll_ReturnsPagedResult_WithDefaultPagination` | 5 ventas | TotalCount=5, Items.Count=5, PageNumber=1 |
| `GetAll_ReturnsCorrectPageSize` | 15 ventas, pageSize=5 | Items.Count=5, PageSize=5 |
| `GetAll_ReturnsCorrectPageNumber` | 25 ventas, page=2, size=10 | Items=11-20, PageNumber=2 |
| `GetAll_ReturnsRemainingItems_OnLastPage` | 25 ventas, page=3, size=10 | Items=5 restantes |
| `GetAll_ReturnsEmpty_WhenPageExceedsTotal` | 1 venta, page=10 | Items vacío |
| `GetAll_WithNullQueryParams_UsesDefaults` | QueryParams=null | PageNumber=1, PageSize=20 |

## GET /api/v1/venta/{id} (`GetById`) — 6 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetById_ReturnsVenta_WhenExists` | ID existente | OkObjectResult, campos correctos (strClaveVenta, strNombreCliente, strNombreUsuario, strEstado, dteFechaHoraCompra) |
| `GetById_ReturnsNotFound_WhenNotExists` | ID=999 | NotFoundResult |
| `GetById_ReturnsNotFound_WithNegativeId` | ID=-1 | NotFoundResult |
| `GetById_ReturnsNotFound_WithZeroId` | ID=0 | NotFoundResult |
| `GetById_ReturnsCorrectVenta_WhenMultipleVentasExist` | 5 ventas, busca CLAVE0003 | Retorna la venta correcta |

## POST /api/v1/venta (`Create`) — 11 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Create_ReturnsCreatedAtActionResult` | Create exitoso | CreatedAtActionResult |
| `Create_ReturnsCreatedWithCorrectRouteName` | Ruta correcta | ActionName == "Get" |
| `Create_ReturnsCreatedWithCorrectRouteValues` | Route values | id > 0 en route values |
| `Create_ReturnsDto_WithIdGreaterThanZero` | ID auto-generado | id > 0 |
| `Create_ReturnsDto_WithGeneratedFields` | Campos automáticos | idVenCatEstado = 1, dteFechaHoraCompra not null, strClaveVenta de 10 chars |
| `Create_ReturnsDto_WithClientName` | Navegación FK | strNombreCliente, strNombreUsuario, strEstado resueltos |
| `Create_PersistsVentaInDatabase` | Persistencia | BD: 1 venta |
| `Create_WithNonExistentCliente_ReturnsBadRequest` | Cliente FK inválido | 400 "El cliente especificado no existe." |
| `Create_WithNonExistentUsuario_ReturnsBadRequest` | Usuario FK inválido | 400 "El usuario especificado no existe." |
| `Create_GeneratesUniqueClaveVenta` | Claves únicas | Dos creates sucesivos producen claves distintas |

## GET /api/v1/venta/buscar (`Search`) — 9 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Search_ByClaveVenta_ReturnsMatching` | Búsqueda parcial por clave | 2 de 3 coincidencias (case-insensitive contains) |
| `Search_ByClaveVenta_ReturnsEmpty_WhenNoMatch` | Sin match | Items vacío |
| `Search_ByNombreCliente_ReturnsMatching` | Búsqueda por nombre de cliente | Solo la venta del cliente que coincide |
| `Search_ByDateRange_ReturnsMatching` | Rango de fechas | 2 de 3 ventas en el rango |
| `Search_ByDateRange_NoTime_ReturnsEntireDay` | Día completo | Captura venta a las 23:59:59 |
| `Search_CombinedFilters_ReturnsMatching` | Filtros combinados | Clave + cliente + fechas → 1 resultado |
| `Search_NoFilters_ReturnsAll` | Todos los filtros null | Retorna todas las ventas |
| `Search_WithPagination_ReturnsCorrectPage` | Paginación en búsqueda | PageNumber y PageSize respetados |

---

## Lecciones aprendidas (extensión a VentaDetalle)

1. **Patrón de autocomplete replicable**: El endpoint `buscarproducto` de VentaDetalle implementa el mismo patrón de autocomplete que `ClienteController`, con búsqueda case-insensitive, orden alfabético, límite configurable (default 10, clamp 1-50) y respuesta con `IEnumerable<Dto>`.

2. **Reutilización de entidades entre módulos**: VentaDetalleService consulta `ProProducto` directamente sin necesidad de inyectar `IProductoService`, lo que evita dependencias circulares y mantiene los servicios desacoplados.

3. **Cálculo server-side**: El campo `decTotalVenta` no se envía desde el cliente — se calcula en el service como `intPiezaVenta * decPrecio` usando el precio actual del producto, garantizando consistencia.

## Resultado general

```
dotnet build -c Release                                             → 0 errores
dotnet test UnitTest --filter "FullyQualifiedName~Venta"            → 33/33
```

## Patrón utilizado

1. `DbContextMock.GetDbContext()` — BD InMemory única por test con `TestDbContext`
2. Seed de FK con `TestDataFactory.CreateClientes()`, `CreateUsers()` y `CreateEstadoVenta()` antes de insertar ventas
3. Service creado via helper: `new VentaService(context, dbResilience)`
4. Assert con FluentAssertions (.Should().BeOfType\<T\>(), validación de propiedades del DTO y DB)
5. Sin hasher — Venta no maneja contraseñas
6. Clave de venta generada con `RandomNumberGenerator.Fill` (10 chars alfanuméricos, retry hasta 10 intentos)
