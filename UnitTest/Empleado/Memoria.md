# Pruebas Unitarias — Empleado (GET, Insert, Update, Delete)

## Archivos involucrados

| Archivo | Tipo | Tests |
|---|---|---|
| `UnitTest/Empleado/InsertTests.cs` | Tests | 10 |
| `UnitTest/Empleado/GetTests.cs` | Tests | 15 |
| `UnitTest/Empleado/UpdateTests.cs` | Tests | 8 |
| `UnitTest/Empleado/DeleteTests.cs` | Tests | 7 |

**Total: 40 tests — 40/40 correctos**

---

## GET /api/v1.0/empleado (`GetAll`) — 3 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetAll_EmptyDatabase_ReturnsEmptyPagedResult` | BD vacía | Items vacío, TotalCount=0, PageNumber=1, PageSize=20 |
| `GetAll_WithData_ReturnsItemsWithDefaultPagination` | 2 empleados | TotalCount=2, Items.Count=2 |
| `GetAll_WithCustomPageSize_ReturnsCorrectSize` | 10 empleados, pageSize=3 | Items.Count=3, PageSize=3 |

## GET /api/v1.0/empleado/{id} (`GetById`) — 4 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetById_ExistingId_ReturnsEmpleado` | ID existente | OkObjectResult, datos correctos |
| `GetById_NonExistentId_ReturnsNotFound` | ID=999 | NotFoundResult |
| `GetById_NegativeId_ReturnsNotFound` | ID=-1 | NotFoundResult |
| `GetById_ZeroId_ReturnsNotFound` | ID=0 | NotFoundResult |

## GET /api/v1.0/empleado/buscar (`Search`) — 8 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Search_ByText_ReturnsMatchingRecords` | Búsqueda por nombre | Resultados contienen "Juan" |
| `Search_ByTextInAPaterno_ReturnsMatchingRecords` | Búsqueda por apellido paterno | Resultados contienen "Pérez" |
| `Search_ByTextInAMaterno_ReturnsMatchingRecords` | Búsqueda por apellido materno | Resultados contienen "García" |
| `Search_ByTipoEmpleado_ReturnsFiltered` | Filtro por tipo | 2 empleados con tipo=1 |
| `Search_ByTextAndTipo_ReturnsIntersection` | Texto + tipo combinado | 1 empleado "Juan" con tipo=1 |
| `Search_NoMatch_ReturnsEmpty` | Sin coincidencias | Items vacío |
| `Search_EmptyText_ReturnsAll` | texto="" | Retorna todos los registros |
| `Search_AppliesPagination` | Paginado | PageSize respetado, TotalCount correcto |

## POST /api/v1.0/empleado (`Create`) — 10 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Create_ReturnsCreatedAtActionResult` | Create exitoso | CreatedAtActionResult |
| `Create_ReturnsCreatedWithCorrectRouteName` | Ruta correcta | ActionName == "Get" |
| `Create_ReturnsCreatedWithCorrectRouteValues` | Route values | id > 0 en route values |
| `Create_ReturnsDto_WithIdGreaterThanZero` | ID auto-generado | id > 0 |
| `Create_ReturnsDto_WithCorrectNombre` | Nombre coincide | strNombre == dto.strNombre |
| `Create_PersistsEmpleadoInDatabase` | Persistencia | DB tiene 1 empleado |
| `Create_TrimsNombre` | `"  Juan  "` | DB: "Juan" |
| `Create_ReturnsBadRequest_WhenCURPExists` | CURP duplicado | BadRequestObjectResult |
| `Create_DoesNotPersistDuplicateCURP` | No persiste duplicado | DB sigue con 1 empleado |
| `Create_NullCURP_AllowsMultipleNulls` | Múltiples CURP null | CreatedAtActionResult (índice único condicional) |

## PUT /api/v1.0/empleado/{id} (`Update`) — 8 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Update_ValidUpdate_ReturnsNoContent` | Update exitoso | NoContentResult |
| `Update_ChangesNombreInDatabase` | Cambiar nombre | DB: strNombre actualizado |
| `Update_TrimsNombre` | `"  Modificado  "` | DB: "Modificado" |
| `Update_RouteIdMismatch_ReturnsBadRequest` | route id ≠ dto.id | BadRequestObjectResult |
| `Update_DuplicateCURP_ReturnsBadRequest` | CURP de otro empleado | BadRequestObjectResult |
| `Update_NonExistentId_ReturnsNotFound` | ID inexistente | NotFoundResult |
| `Update_ThrowsDbUpdateConcurrencyException_WhenRowVersionMismatch` | RowVersion incorrecto | ConflictObjectResult |
| `Update_AllowsSelfRename` | Mismo nombre | NoContentResult |

## DELETE /api/v1.0/empleado/{id} (`Delete`) — 7 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Delete_ValidDelete_ReturnsOk` | Delete exitoso | OkObjectResult |
| `Delete_RemovesEmpleadoFromDatabase` | Empleado eliminado | DB: 0 empleados |
| `Delete_OnlyRemovesTargetEmpleado` | Múltiples, elimina uno | DB: 1 empleado restante |
| `Delete_RouteIdMismatch_ReturnsBadRequest` | route id ≠ dto.id | BadRequestObjectResult |
| `Delete_RouteIdMismatch_CheckedBeforeExistence` | ID mismatch sin empleado | BadRequestObjectResult (se valida antes) |
| `Delete_NonExistentId_ReturnsNotFound` | ID inexistente | NotFoundResult |
| `Delete_ThrowsDbUpdateConcurrencyException_WhenRowVersionMismatch` | RowVersion incorrecto | ConflictObjectResult |

---

## Resultado general

```
dotnet build -c Release                                             → 0 errores
dotnet test UnitTest --filter "FullyQualifiedName~Empleado"         → 40/40
```

## Patrón utilizado

1. `DbContextMock.GetDbContext()` — BD InMemory única por test
2. Seed de datos con `context.EmpEmpleado.Add`
3. Controller creado via helper: `new EmpleadoController(new EmpleadoService(context, dbResilience))`
4. Assert con FluentAssertions (.Should().BeOfType\<T\>(), validación de propiedades del DTO y DB)
5. Búsqueda combinada por texto (nombre + apaterno + amaterno) y tipo de empleado
6. CURP único con soporte para múltiples nulls (filtro `WHERE strCURP == dto.strCURP AND strCURP != null`)
