# Pruebas Unitarias — Producto (GET, Insert, Update, Delete)

## Archivos involucrados

| Archivo | Tipo | Tests |
|---|---|---|
| `UnitTest/Common/TestDataFactory.cs` | Utilidad | Métodos `CreateProducto*` |
| `UnitTest/Producto/InsertTests.cs` | Tests | 15 |
| `UnitTest/Producto/GetTests.cs` | Tests | 12 |
| `UnitTest/Producto/UpdateTests.cs` | Tests | 10 |
| `UnitTest/Producto/DeleteTests.cs` | Tests | 9 |

**Total: 46 tests — 46/46 correctos**

---

## GET /api/v1.0/producto (`GetAll`) — 7 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetAll_ReturnsEmptyPagedResult_WhenNoProductos` | BD vacía | Items vacío, TotalCount=0, PageNumber=1, PageSize=20, TotalPages=0 |
| `GetAll_ReturnsPagedResult_WithDefaultPagination` | 5 productos, sin params | TotalCount=5, Items.Count=5 |
| `GetAll_ReturnsCorrectPageSize` | 50 productos, pageSize=10 | Items.Count=10, PageSize=10 |
| `GetAll_ReturnsCorrectPageNumber` | 30 productos, page=2, size=10 | Items.Count=10, PageNumber=2, primero=producto11, último=producto20 |
| `GetAll_ReturnsRemainingItems_OnLastPage` | 25 productos, page=3, size=10 | Items.Count=5, primero=producto21, último=producto25 |
| `GetAll_ReturnsEmpty_WhenPageExceedsTotal` | 5 productos, page=10 | Items vacío, TotalCount=5 |
| `GetAll_WithNullQueryParams_UsesDefaults` | QueryParams=null | PageNumber=1, PageSize=20 |

## GET /api/v1.0/producto/{id} (`GetById`) — 5 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetById_ReturnsProducto_WhenExists` | ID existente | OkObjectResult, datos del producto correctos |
| `GetById_ReturnsNotFound_WhenNotExists` | ID=999 | NotFoundResult |
| `GetById_ReturnsNotFound_WithNegativeId` | ID=-1 | NotFoundResult |
| `GetById_ReturnsNotFound_WithZeroId` | ID=0 | NotFoundResult |
| `GetById_ReturnsCorrectProducto_WhenMultipleProductosExist` | 5 productos, busca producto3 | Retorna el producto correcto |

## POST /api/v1.0/producto (`Create`) — 15 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Create_ReturnsCreatedAtActionResult` | Create exitoso | CreatedAtActionResult |
| `Create_ReturnsCreatedWithCorrectRouteName` | Ruta correcta | ActionName == "Get" |
| `Create_ReturnsCreatedWithCorrectRouteValues` | Route values | id > 0 en route values |
| `Create_ReturnsDto_WithIdGreaterThanZero` | ID auto-generado | id > 0 |
| `Create_ReturnsDto_WithCorrectNombre` | Nombre coincide | strNombreProducto == dto.strNombreProducto |
| `Create_ReturnsDto_WithCorrectExistencia` | Existencia coincide | intNumeroExistencia == 25 |
| `Create_ReturnsDto_WithCorrectPrecio` | Precio coincide | decPrecio == 149.99m |
| `Create_PersistsProductoInDatabase` | Persistencia | DB tiene 1 producto |
| `Create_TrimsNombreProducto` | `"  nombre  "` | DB: "nombre" |
| `Create_WithImagenNull_Succeeds` | strURLImagen null | CreatedAtActionResult (campo nullable) |
| `Create_WithDescripcionNull_Succeeds` | strDescripcion null | CreatedAtActionResult (campo nullable) |
| `Create_AllowsDuplicateNombreProducto` | Mismo nombre, distinto precio | CreatedAtActionResult (nombres no son únicos) |

## PUT /api/v1.0/producto/{id} (`Update`) — 10 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Update_ReturnsNoContent_WhenSuccessful` | Update exitoso | NoContentResult |
| `Update_ChangesNombre_InDatabase` | Cambiar nombre | DB: strNombreProducto actualizado |
| `Update_ChangesExistencia_InDatabase` | Cambiar existencia | DB: intNumeroExistencia == 50 |
| `Update_ChangesPrecio_InDatabase` | Cambiar precio | DB: decPrecio == 250.00m |
| `Update_TrimsNombreProducto` | `"  Con Espacios  "` | DB: "Con Espacios" |
| `Update_ReturnsBadRequest_WhenIdMismatch` | route id ≠ dto.id | BadRequestObjectResult |
| `Update_ReturnsBadRequest_WithCorrectErrorMessage_IdMismatch` | Mensaje error | "El ID del producto no coincide." |
| `Update_ReturnsNotFound_WhenProductoDoesNotExist` | ID inexistente | NotFoundResult |
| `Update_ReturnsConflict_WhenRowVersionMismatch` | RowVersion incorrecto | ConflictResult |

## DELETE /api/v1.0/producto/{id} (`Delete`) — 9 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Delete_ReturnsOk_WhenSuccessful` | Delete exitoso | OkResult |
| `Delete_RemovesProductoFromDatabase` | Producto eliminado | DB: 0 productos |
| `Delete_RemovesOnlyTargetProducto` | Múltiples, elimina uno | DB: 1 producto restante |
| `Delete_ReturnsBadRequest_WhenIdMismatch` | route id ≠ dto.id | BadRequestObjectResult |
| `Delete_ReturnsBadRequest_WithCorrectErrorMessage_IdMismatch` | Mensaje error | "El ID de la ruta no coincide con el ID del cuerpo." |
| `Delete_ReturnsNotFound_WhenProductoDoesNotExist` | ID inexistente | NotFoundResult |
| `Delete_ReturnsBadRequest_WhenIdMismatch_BeforeCheckingExistence` | ID mismatch sin producto | BadRequestObjectResult (se valida antes que existencia) |
| `Delete_ReturnsConflict_WhenRowVersionMismatch` | RowVersion incorrecto | ConflictResult |

---

## Lecciones aprendidas (consumo desde VentaDetalle)

1. **ProProducto como entidad compartida**: La entidad `ProProducto` es consultada directamente desde `VentaDetalleService` para el cálculo de `decTotalVenta` (via `decPrecio`) y para el endpoint de autocomplete `buscarproducto`. Esto demuestra que las entidades pueden ser reutilizadas entre servicios sin necesidad de inyectar dependencias de servicio a servicio.

2. **Proyección parcial desde Service externo**: VentaDetalleService consulta `ProProducto` con `.Select(p => new { p.decPrecio })` — una proyección mínima para evitar traer datos innecesarios al calcular `decTotalVenta`.

3. **Formato de autocomplete**: El endpoint `buscarproducto` de VentaDetalle expone datos de Producto con el formato `"{strNombreProducto} | #: {intNumeroExistencia} | $: {decPrecio}"` usando un DTO específico (`ProProductoAutocompleteDto`) que no duplica el DTO de respuesta completo.

## Resultado general

```
dotnet build -c Release                                           → 0 errores
dotnet test UnitTest --filter "FullyQualifiedName~Producto"       → 46/46
```

## Patrón utilizado

1. `DbContextMock.GetDbContext()` — BD InMemory única por test
2. Seed de datos con `context.ProProducto.Add` / `TestDataFactory.CreateProductos()`
3. Controller creado via helper: `new ProductoController(new ProductoService(context, dbResilience))`
4. Assert con FluentAssertions (.Should().BeOfType\<T\>(), validación de propiedades del DTO y DB)
5. Sin hasher — Producto no maneja contraseñas
6. Nombres duplicados permitidos (índice no único en `strNombreProducto`)
