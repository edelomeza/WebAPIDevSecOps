# Memoria de Pruebas de Integración — Usuario

## 1. Introducción

Este documento describe el conjunto de pruebas de integración para el controlador `UsuarioController` (endpoints `api/v{version}/usuario`). Las pruebas validan el correcto funcionamiento del pipeline completo: enrutamiento, autenticación JWT, autorización por rol, validación de DTOs, lógica de negocio, concurrencia y persistencia.

Tecnologías utilizadas:
- **xUnit** + **FluentAssertions** como framework de pruebas
- **WebApplicationFactory\<Program\>** para hostear la aplicación en memoria
- **EF Core InMemory** como sustituto de SQL Server
- **JwtTestConfig** (clase compartida en `UnitTest/Common/`) para generación de tokens

## 2. Arquitectura de pruebas

```
WebApplicationFactory<Program>
  └─ WithWebHostBuilder
       ├─ UseSetting("UseInMemoryDatabase", "true")  → BD InMemory
       ├─ UseSetting("Jwt:Key", ...)                   → clave JWT de prueba
       └─ UseSetting("Jwt:Issuer"/"Jwt:Audience", ...)
  └─ CreateClient() → HttpClient autenticado
```

### Aislamiento entre pruebas

Se implementó `IAsyncLifetime` con un método `InitializeAsync()` que limpia la tabla `SegUsuario` antes de cada test. Esto evita contaminación entre pruebas dado que `IClassFixture` comparte la misma instancia de `WebApplicationFactory` (y por tanto la misma BD InMemory) entre todos los tests de la clase.

```csharp
public async Task InitializeAsync()
{
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.SegUsuario.RemoveRange(db.SegUsuario);
    await db.SaveChangesAsync();
}
```

### Helpers

- **`CreateUserAsync(uniqueName?)`**: Crea un usuario vía POST, luego hace GET para obtener el `RowVersion` (no se devuelve en la respuesta de creación). Retorna `SegUsuarioDto` completo.
- **`GetUserAsync(id)`**: GET por id, retorna `SegUsuarioDto`.
- **`AdminToken`**: Token JWT con rol `Admin` (requerido por la policy `AdminOnly` en todos los endpoints).

## 3. Clase utilitaria compartida

Se creó `UnitTest/Common/JwtTestConfig.cs` para centralizar las constantes JWT que antes estaban duplicadas en `SecurityTests.cs` y ahora también en `IntegrationTests.cs`:

```csharp
public static class JwtTestConfig
{
    public const string Key = "01123581321345589144233377610987";
    public const string Issuer = "edelmeza.com";
    public const string Audience = "edelmeza.com";
    public static string AdminToken => TokenHelper.GenerateValidToken(Key, Issuer, Audience);
    public static string UserToken => TokenHelper.GenerateTokenWithRole(Key, Issuer, Audience, "User");
}
```

## 4. Escenarios cubiertos

### GET /api/v1/usuario (GetAll) — 8 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 1 | `EmptyDatabase_ReturnsEmptyPagedResult` | Items vacío, TotalCount=0, PageNumber=1, PageSize=20, TotalPages=0 |
| 2 | `WithUsers_ReturnsDefaultPagination` | Items=3, PageSize=20 (default) |
| 3 | `WithCustomPageSize_ReturnsCorrectSize` | pageSize=5, Items=5 de 15 total |
| 4 | `WithCustomPageNumber_ReturnsCorrectPage` | pageNumber=2, pageSize=10 |
| 5 | `OnLastPage_ReturnsRemainingItems` | Página 3 de 25 registros → Items=5 |
| 6 | `WhenPageExceedsTotal_ReturnsEmpty` | page=10, pageSize=10 → Items vacío |
| 7 | `DoesNotExposePassword` | JSON response sin `strPWD` |
| 8 | `HasCacheControlHeader` | `Cache-Control: private, max-age=30` |

### GET /api/v1/usuario/{id} (GetById) — 6 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 9 | `ExistingUser_ReturnsUser` | 200, datos correctos |
| 10 | `NonExistentId_ReturnsNotFound` | 404 (id 9999) |
| 11 | `NegativeId_ReturnsNotFound` | 404 (id -1) |
| 12 | `ZeroId_ReturnsNotFound` | 404 (id 0) |
| 13 | `MultipleUsers_ReturnsCorrectUser` | Entre 3 usuarios, obtiene el del medio |
| 14 | `DoesNotExposePassword` | JSON sin `strPWD` |

### POST /api/v1/usuario (Create) — 12 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 15 | `ValidDto_Returns201WithLocationHeader` | 201 + Location → `/api/v1/Usuario/{id}` |
| 16 | `ValidDto_ReturnsSegUsuarioDtoWithId` | id > 0 |
| 17 | `ValidDto_ReturnsCorrectNombreAndCorreo` | Campos coinciden |
| 18 | `ValidDto_CreatesUserInDatabase` | GET posterior confirma persistencia |
| 19 | `ValidDto_DoesNotExposePassword` | JSON sin `strPWD` |
| 20 | `DuplicateNombre_Returns400` | Mismo nombre → 400 |
| 21 | `EmptyNombre_Returns400` | string vacío → 400 |
| 22 | `NombreTooLong_Returns400` | 51 caracteres → 400 |
| 23 | `InvalidNombreCharacters_Returns400` | `@`, `!` → 400 (regex) |
| 24 | `PasswordTooShort_Returns400` | 7 caracteres → 400 (MinLength 8) |
| 25 | `InvalidEmail_Returns400` | Formato inválido → 400 |
| 26 | `TrimsNombre` | `"  name  "` → `"name"` |

### PUT /api/v1/usuario/{id} (Update) — 13 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 27 | `ValidUpdate_Returns204` | 204 NoContent |
| 28 | `ValidUpdate_ChangesNombreAndCorreo` | GET posterior confirma cambios |
| 29 | `WithNewPassword_Returns204` | strPWD provisto → 204 |
| 30 | `WithoutPassword_KeepsExistingData` | strPWD omitido → datos se actualizan, password intacto |
| 31 | `RouteIdMismatch_Returns400` | Route id=1, body id=999 → 400 |
| 32 | `DuplicateNombre_Returns400` | Cambiar a nombre existente → 400 |
| 33 | `NonExistentId_Returns404` | id 9999 → 404 |
| 34 | `StaleRowVersion_Returns409` | RowVersion incorrecto → 409 Conflict |
| 35 | `EmptyNombre_Returns400` | string vacío → 400 |
| 36 | `NombreTooLong_Returns400` | 51 caracteres → 400 |
| 37 | `InvalidEmail_Returns400` | Formato inválido → 400 |
| 38 | `TrimsNombre` | `"  name  "` → `"name"` |
| 39 | `SelfRename_AllowsSameName` | Mismo nombre → 204 (no hay conflicto) |

### DELETE /api/v1/usuario/{id} (Delete) — 6 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 40 | `ValidDelete_Returns200` | 200 OK |
| 41 | `ValidDelete_RemovesUserFromDatabase` | GET posterior → 404 |
| 42 | `RouteIdMismatch_Returns400` | Route id=1, body id=999 → 400 |
| 43 | `NonExistentId_Returns404` | id 9999 → 404 |
| 44 | `OnlyRemovesTargetUser` | 2 usuarios, elimina 1 → el otro sigue accesible |
| 45 | `StaleRowVersion_Returns409` | RowVersion incorrecto → 409 Conflict |

### Full Lifecycle — 1 prueba

| # | Prueba | Flujo |
|---|--------|-------|
| 46 | `CreateGetUpdateGetDelete_CompleteFlow` | Create (201) → Get (200) → Update (204) → Get (200, datos cambiados) → Delete (200) → Get (404) |

**Total: 50 pruebas**

## 5. Problemas detectados durante la implementación

### 5.1 Contaminación entre pruebas (test pollution)

**Problema**: `IClassFixture<WebApplicationFactory<Program>>` comparte la misma instancia del factory (y por tanto la misma BD InMemory) entre todos los tests del fixture. Los tests que verifican cantidades exactas fallaban porque los tests anteriores ya habían insertado datos.

**Solución**: Implementar `IAsyncLifetime` con limpieza de la tabla `SegUsuario` en `InitializeAsync()`, que se ejecuta antes de cada test.

### 5.2 Cache-Control para Location.Client

**Problema**: El atributo `[ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client)]` produce `Cache-Control: private, max-age=30`, con `Public = false`. La aserción inicial esperaba `Public = true`.

**Solución**: Cambiar la aserción a `CacheControl.Private.Should().BeTrue()`.

### 5.3 Location header con PascalCase

**Problema**: `CreatedAtAction` genera la URL usando `[controller]` que toma el nombre de la clase `UsuarioController`, resultando en `/api/v1/Usuario/{id}` (con mayúscula). La aserción buscaba `/api/v1/usuario/` (minúscula).

**Solución**: Corregir el string de búsqueda a `"/api/v1/Usuario/"`.

## 6. Resultado de ejecución

```
Correctas! - Con error: 0, Superado: 50, Omitido: 0, Total: 50, Duración: 59s
```

Todos los tests se ejecutan con el comando:

```powershell
dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build
```

## 7. Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `IntegrationTest/Usuarios/IntegrationTests.cs` | Clase con 50 tests |
| `UnitTest/Common/JwtTestConfig.cs` | Constantes JWT compartidas |
| `UnitTest/Common/TokenHelper.cs` | Generación de tokens JWT |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs y entidades |
