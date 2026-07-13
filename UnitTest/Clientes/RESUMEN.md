# Pruebas Unitarias — Cliente (GET, Insert, Update, Delete, Query)

## Archivos involucrados

| Archivo | Tipo | Tests |
|---|---|---|
| `UnitTest/Common/TestDataFactory.cs` | Utilidad | Métodos `CreateCliente*` |
| `UnitTest/Common/TestDbContext.cs` | Utilidad | Auto-set RowVersion para CliCliente |
| `UnitTest/Clientes/InsertTests.cs` | Tests | 15 |
| `UnitTest/Clientes/GetTests.cs` | Tests | 12 |
| `UnitTest/Clientes/QueryTests.cs` | Tests | 8 |
| `UnitTest/Clientes/UpdateTests.cs` | Tests | 12 |
| `UnitTest/Clientes/DeleteTests.cs` | Tests | 8 |

**Total: 56 tests — 56/56 correctos**

---

## GET /api/v1.0/cliente (`GetAll`) — 7 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetAll_ReturnsEmptyPagedResult_WhenNoClientes` | BD vacía | Items vacío, TotalCount=0, PageNumber=1, PageSize=20 |
| `GetAll_ReturnsPagedResult_WithDefaultPagination` | 5 clientes, sin params | TotalCount=5, Items.Count=5 |
| `GetAll_ReturnsCorrectPageSize` | 50 clientes, pageSize=10 | Items.Count=10, PageSize=10 |
| `GetAll_ReturnsCorrectPageNumber` | 30 clientes, page=2, size=10 | Items.Count=10, PageNumber=2 |
| `GetAll_ReturnsRemainingItems_OnLastPage` | 25 clientes, page=3, size=10 | Items.Count=5 |
| `GetAll_ReturnsEmpty_WhenPageExceedsTotal` | 5 clientes, page=10 | Items vacío, TotalCount=5 |
| `GetAll_WithNullQueryParams_UsesDefaults` | QueryParams=null | PageNumber=1, PageSize=20 |

## GET /api/v1.0/cliente/{id} (`GetById`) — 5 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetById_ReturnsCliente_WhenExists` | ID existente | OkObjectResult, datos correctos |
| `GetById_ReturnsNotFound_WhenNotExists` | ID=999 | NotFoundResult |
| `GetById_ReturnsNotFound_WithNegativeId` | ID=-1 | NotFoundResult |
| `GetById_ReturnsNotFound_WithZeroId` | ID=0 | NotFoundResult |
| `GetById_ReturnsCorrectCliente_WhenMultipleClientesExist` | 5 clientes, busca el 3ro | Retorna el cliente correcto |

## POST /api/v1.0/cliente (`Create`) — 15 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Create_ReturnsCreatedAtActionResult` | Create exitoso | CreatedAtActionResult |
| `Create_ReturnsCreatedWithCorrectRouteName` | Ruta correcta | ActionName == "Get" |
| `Create_ReturnsCreatedWithCorrectRouteValues` | Route values | id > 0 en route values |
| `Create_ReturnsDto_WithIdGreaterThanZero` | ID auto-generado | id > 0 |
| `Create_ReturnsDto_WithCorrectNombre` | Nombre coincide | strNombreCliente == dto.strNombreCliente |
| `Create_ReturnsDto_WithCorrectCorreo` | Email coincide | strCorreoElectronico == dto.strCorreoElectronico |
| `Create_ReturnsDto_WithCorrectTelefono` | Teléfono coincide | strNumeroTelefono == dto.strNumeroTelefono |
| `Create_PersistsClienteInDatabase` | Persistencia | DB tiene 1 cliente |
| `Create_TrimsNombreCliente` | `"  nombre  "` | DB: "nombre" |
| `Create_TrimsCorreoElectronico` | `"  a@a.com  "` | DB: "a@a.com" |
| `Create_ReturnsBadRequest_WhenCorreoExists` | Correo duplicado | BadRequestObjectResult |
| `Create_ReturnsBadRequest_WithCorrectErrorMessage_WhenCorreoExists` | Mensaje error | "El correo electrónico ya está registrado." |
| `Create_DoesNotPersist_WhenCorreoDuplicado` | No persiste duplicado | DB sigue con 0/1 clientes |
| `Create_AllowsDuplicateNombreCliente` | Mismo nombre, distinto correo | CreatedAtActionResult (nombres no son únicos) |
| `Create_WithDireccionNull_Succeeds` | Dirección nula | CreatedAtActionResult (campo nullable) |

## GET /api/v1.0/cliente/buscar (`SearchByName`) — 7 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `SearchByName_ReturnsMatchingClientes` | Búsqueda parcial | Resultados contienen "Eduardo" |
| `SearchByName_ReturnsEmpty_WhenNoMatch` | Sin coincidencias | Items vacío, TotalCount=0 |
| `SearchByName_IsCaseInsensitive` | Case insensitive | "eduardo" encuentra "Eduardo" |
| `SearchByName_WithPagination_ReturnsCorrectPage` | Paginado | PageSize respetado |
| `SearchByName_EmptyTexto_ReturnsBadRequest` | texto vacío | BadRequest |
| `SearchByName_WithWhitespace_ReturnsBadRequest` | texto whitespace | BadRequest |
| `SearchByName_SpecialChars_ReturnsMatch` | Caracteres especiales | `"María José"` encuentra match |

## PUT /api/v1.0/cliente/{id} (`Update`) — 12 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Update_ReturnsNoContent_WhenSuccessful` | Update exitoso | NoContentResult |
| `Update_ChangesNombre_InDatabase` | Cambiar nombre | DB: strNombreCliente actualizado |
| `Update_ChangesCorreo_InDatabase` | Cambiar email | DB: strCorreoElectronico actualizado |
| `Update_ChangesTelefono_InDatabase` | Cambiar teléfono | DB: strNumeroTelefono actualizado |
| `Update_ChangesDireccion_InDatabase` | Cambiar dirección | DB: strDireccionCliente actualizado |
| `Update_SetsDireccionToNull` | Dirección a null | DB: strDireccionCliente == null |
| `Update_TrimsNombreCliente` | `"  conEspacios  "` | DB: "conEspacios" |
| `Update_TrimsCorreo` | `"  correo@test.com  "` | DB: "correo@test.com" |
| `Update_AllowsSelfRename_WithSameCorreo` | Mismo correo | NoContentResult |
| `Update_ReturnsBadRequest_WhenIdMismatch` | route id ≠ dto.id | BadRequestObjectResult |
| `Update_ReturnsBadRequest_WithCorrectErrorMessage_IdMismatch` | Mensaje error | "El ID del cliente no coincide." |
| `Update_ReturnsBadRequest_WhenCorreoAlreadyExists` | Otro cliente con ese correo | BadRequestObjectResult |
| `Update_ReturnsNotFound_WhenClienteDoesNotExist` | ID inexistente | NotFoundResult |
| `Update_ReturnsConflict_WhenRowVersionMismatch` | RowVersion incorrecto | ConflictResult |

## DELETE /api/v1.0/cliente/{id} (`Delete`) — 8 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Delete_ReturnsOk_WhenSuccessful` | Delete exitoso | OkResult |
| `Delete_RemovesClienteFromDatabase` | Cliente eliminado | DB: 0 clientes |
| `Delete_RemovesOnlyTargetCliente` | Múltiples, elimina uno | DB: 1 cliente restante |
| `Delete_ReturnsBadRequest_WhenIdMismatch` | route id ≠ dto.id | BadRequestObjectResult |
| `Delete_ReturnsBadRequest_WithCorrectErrorMessage_IdMismatch` | Mensaje error | "El ID de la ruta no coincide con el ID del cuerpo." |
| `Delete_ReturnsNotFound_WhenClienteDoesNotExist` | ID inexistente | NotFoundResult |
| `Delete_ReturnsBadRequest_WhenIdMismatch_BeforeCheckingExistence` | ID mismatch sin cliente | BadRequestObjectResult (se valida antes que existencia) |
| `Delete_ReturnsConflict_WhenRowVersionMismatch` | RowVersion incorrecto | ConflictResult |

---

## Resultado general

```
dotnet build -c Release                                             → 0 errores
dotnet test UnitTest --filter "FullyQualifiedName~Clientes"         → 56/56
```

## Patrón utilizado

1. `DbContextMock.GetDbContext()` — BD InMemory única por test
2. Seed de datos con `context.CliCliente.Add` / `TestDataFactory.CreateCliente*()`
3. Controller creado via helper: `new ClienteController(new ClienteService(context, dbResilience))`
4. Assert con FluentAssertions (.Should().BeOfType\<T\>(), validación de propiedades del DTO y DB)
5. `TestDbContext` sobreescribe `SaveChangesAsync` para auto-asignar `RowVersion` en `CliCliente`
