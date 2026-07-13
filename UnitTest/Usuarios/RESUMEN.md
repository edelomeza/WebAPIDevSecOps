# Pruebas Unitarias — Usuario (GET, Insert, Update, Delete)

## Archivos involucrados

| Archivo | Tipo | Tests |
|---|---|---|
| `UnitTest/Common/TestDataFactory.cs` | Utilidad | +`CreateUsers(int count, string? passwordHash)` |
| `UnitTest/Usuarios/GetTests.cs` | Tests | 13 |
| `UnitTest/Usuarios/InsertTests.cs` | Tests | 15 |
| `UnitTest/Usuarios/UpdateTests.cs` | Tests | 14 |
| `UnitTest/Usuarios/DeleteTests.cs` | Tests | 8 |

**Total: 50 tests — 50/50 correctos**

---

## GET /api/v1.0/Usuario (`GetAll`) — 8 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetAll_ReturnsEmptyPagedResult_WhenNoUsers` | BD vacía | Items vacío, TotalCount=0, PageNumber=1, PageSize=20 |
| `GetAll_ReturnsPagedResult_WithDefaultPagination` | 5 usuarios, sin params | TotalCount=5, Items.Count=5 |
| `GetAll_ReturnsCorrectPageSize` | 50 usuarios, pageSize=10 | Items.Count=10, PageSize=10 |
| `GetAll_ReturnsCorrectPageNumber` | 30 usuarios, page=2, size=10 | Items.Count=10, PageNumber=2, primero=user11, último=user20 |
| `GetAll_ReturnsRemainingItems_OnLastPage` | 25 usuarios, page=3, size=10 | Items.Count=5, primero=user21, último=user25 |
| `GetAll_ReturnsEmpty_WhenPageExceedsTotal` | 5 usuarios, page=10 | Items vacío, TotalCount=5 |
| `GetAll_WithNullQueryParams_UsesDefaults` | QueryParams=null | PageNumber=1, PageSize=20 |
| `GetAll_DoesNotExposePassword` | Verificar DTO | Propiedad strPWD no existe en el DTO |

## GET /api/v1.0/Usuario/{id} (`GetById`) — 5 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `GetById_ReturnsUser_WhenExists` | ID existente | OkObjectResult, datos del usuario correctos |
| `GetById_ReturnsNotFound_WhenNotExists` | ID=999 | NotFoundResult |
| `GetById_ReturnsNotFound_WithNegativeId` | ID=-1 | NotFoundResult |
| `GetById_ReturnsNotFound_WithZeroId` | ID=0 | NotFoundResult |
| `GetById_ReturnsCorrectUser_WhenMultipleUsersExist` | 5 usuarios, busca user3 | Retorna el usuario correcto |

## POST /api/v1.0/Usuario (`Create`) — 15 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Create_ReturnsCreatedAtActionResult` | Create exitoso | CreatedAtActionResult |
| `Create_ReturnsCreatedWithCorrectRouteName` | Ruta correcta | ActionName == "Get" |
| `Create_ReturnsCreatedWithCorrectRouteValues` | Route values | id > 0 en route values |
| `Create_ReturnsDto_WithIdGreaterThanZero` | ID auto-generado | id > 0 |
| `Create_ReturnsDto_WithCorrectNombre` | Nombre coincide | strNombre == dto.strNombre |
| `Create_ReturnsDto_WithCorrectCorreo` | Email coincide | strCorreoElectronico == dto.strCorreoElectronico |
| `Create_DtoDoesNotContainStrPWD` | Seguridad | No existe propiedad strPWD en DTO |
| `Create_PersistsUserInDatabase` | Persistencia | DB tiene 1 usuario |
| `Create_SetsFechaRegistro` | Auditoría | dteFechaRegistro no es null |
| `Create_PasswordIsHashed_WhenCreated` | Hashing | strPWD en DB ≠ contraseña original |
| `Create_TrimsNombre` | Limpieza | `"  user  "` → `"user"` |
| `Create_TrimsCorreoElectronico` | Limpieza | `"  a@a.com  "` → `"a@a.com"` |
| `Create_ReturnsBadRequest_WhenUsernameExists` | Duplicado | BadRequestObjectResult |
| `Create_ReturnsBadRequest_WithCorrectErrorMessage` | Mensaje error | mensaje == "El nombre de usuario ya existe." |
| `Create_DoesNotPersistDuplicateUser` | No persiste duplicado | DB sigue con 1 usuario |

## PUT /api/v1.0/Usuario/{id} (`Update`) — 14 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Update_ReturnsNoContent_WhenSuccessful` | Update exitoso | NoContentResult |
| `Update_ChangesNombre_InDatabase` | Cambiar nombre | DB: strNombre == "nombreActualizado" |
| `Update_ChangesCorreo_InDatabase` | Cambiar email | DB: strCorreoElectronico actualizado |
| `Update_DoesNotChangePassword_WhenPasswordIsNull` | strPWD = null | DB: password original intacto |
| `Update_DoesNotChangePassword_WhenPasswordIsEmpty` | strPWD = "" | DB: password original intacto |
| `Update_HashesPassword_WhenProvided` | Nuevo password | HashPassword invocado 1 vez |
| `Update_TrimsNombre` | `"  conEspacios  "` | DB: "conEspacios" |
| `Update_TrimsCorreo` | `"  correo@test.com  "` | DB: "correo@test.com" |
| `Update_AllowsSelfRename` | Mismo nombre | NoContentResult |
| `Update_ReturnsBadRequest_WhenIdMismatch` | route id ≠ dto.id | BadRequestObjectResult |
| `Update_ReturnsBadRequest_WithCorrectErrorMessage_IdMismatch` | Mensaje error | "El ID del usuario no coincide." |
| `Update_ReturnsBadRequest_WhenUsernameAlreadyExists` | Otro usuario con ese nombre | BadRequestObjectResult |
| `Update_ReturnsNotFound_WhenUserDoesNotExist` | ID inexistente | NotFoundResult |
| `Update_ReturnsConflict_WhenRowVersionMismatch` | RowVersion incorrecto | ConflictResult |

## DELETE /api/v1.0/Usuario/{id} (`Delete`) — 8 tests

| Test | Escenario | Assert clave |
|---|---|---|
| `Delete_ReturnsOk_WhenSuccessful` | Delete exitoso | OkResult |
| `Delete_RemovesUserFromDatabase` | Usuario eliminado | DB: 0 usuarios |
| `Delete_RemovesOnlyTargetUser` | Múltiples usuarios, elimina uno | DB: 1 usuario restante |
| `Delete_ReturnsBadRequest_WhenIdMismatch` | route id ≠ dto.id | BadRequestObjectResult |
| `Delete_ReturnsBadRequest_WithCorrectErrorMessage_IdMismatch` | Mensaje error | "El ID de la ruta no coincide con el ID del cuerpo." |
| `Delete_ReturnsNotFound_WhenUserDoesNotExist` | ID inexistente | NotFoundResult |
| `Delete_ReturnsBadRequest_WhenIdMismatch_BeforeCheckingExistence` | ID mismatch sin usuario | BadRequestObjectResult (se valida antes que existencia) |
| `Delete_ReturnsConflict_WhenRowVersionMismatch` | RowVersion incorrecto | ConflictResult |

---

## Resultado general

```
dotnet build -c Release                                           → 0 errores
dotnet test UnitTest --filter "GetTests"                          → 13/13
dotnet test UnitTest --filter "InsertTests"                       → 15/15
dotnet test UnitTest --filter "UpdateTests"                       → 14/14
dotnet test UnitTest --filter "DeleteTests"                       →  8/8
```

## Patrón utilizado

1. `DbContextMock.GetDbContext()` — BD InMemory única por test
2. Seed de datos con `context.SegUsuario.Add` / `TestDataFactory.CreateUsers()`
3. Controller creado via helper: `new UsuarioController(new UsuarioService(context, hasherMock, dbResilience))`
4. Assert con FluentAssertions (.Should().BeOfType\<T\>(), validación de propiedades del DTO y DB)
