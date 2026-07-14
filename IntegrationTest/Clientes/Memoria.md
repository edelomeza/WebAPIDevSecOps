# Memoria de Pruebas de Integración — Cliente

## 1. Introducción

Este documento describe el conjunto de pruebas de integración para el controlador `ClienteController` (endpoints `api/v{version}/cliente`). Las pruebas validan el correcto funcionamiento del pipeline completo: enrutamiento, autenticación JWT, autorización por rol, validación de DTOs, lógica de negocio, concurrencia y persistencia.

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

Se implementó `IAsyncLifetime` con un método `InitializeAsync()` que limpia la tabla `CliCliente` antes de cada test. Esto evita contaminación entre pruebas dado que `IClassFixture` comparte la misma instancia de `WebApplicationFactory` (y por tanto la misma BD InMemory) entre todos los tests de la clase.

```csharp
public async Task InitializeAsync()
{
    TokenBlacklist.Clear();
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.CliCliente.RemoveRange(db.CliCliente);
    await db.SaveChangesAsync();
}
```

### Helpers

- **`CreateClienteAsync(uniqueName?)`**: Crea un cliente vía POST, luego hace GET para obtener el `RowVersion` (no se devuelve en la respuesta de creación). Retorna `CliClienteDto` completo.
- **`AdminToken`**: Token JWT con rol `Admin` (requerido por la policy `AdminOnly` en todos los endpoints).

## 3. Clase utilitaria compartida

Se reutiliza `UnitTest/Common/JwtTestConfig.cs` para centralizar las constantes JWT:

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

### GET /api/v1/cliente (GetAll) — 8 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 1 | `EmptyDatabase_ReturnsEmptyPagedResult` | Items vacío, TotalCount=0, PageNumber=1, PageSize=20, TotalPages=0 |
| 2 | `WithClientes_ReturnsDefaultPagination` | Items=3, PageSize=20 (default) |
| 3 | `WithCustomPageSize_ReturnsCorrectSize` | pageSize=5, Items=5 de 15 total |
| 4 | `WithCustomPageNumber_ReturnsCorrectPage` | pageNumber=2, pageSize=10 |
| 5 | `OnLastPage_ReturnsRemainingItems` | Página 3 de 25 registros → Items=5 |
| 6 | `WhenPageExceedsTotal_ReturnsEmpty` | page=10, pageSize=10 → Items vacío |
| 7 | `HasCacheControlHeader` | `Cache-Control: no-store` |
| 8 | `GetAll_WithClientes_ReturnsDefaultPagination` | Items=3, TotalCount=3 |

### GET /api/v1/cliente/{id} (GetById) — 6 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 9 | `ExistingCliente_ReturnsCliente` | 200, datos correctos |
| 10 | `NonExistentId_ReturnsNotFound` | 404 (id 9999) |
| 11 | `NegativeId_ReturnsNotFound` | 404 (id -1) |
| 12 | `ZeroId_ReturnsNotFound` | 404 (id 0) |
| 13 | `MultipleClientes_ReturnsCorrectCliente` | Entre 3 clientes, obtiene el del medio |
| 14 | `SearchByName_ReturnsMatchingClientes` | Búsqueda por texto parcial |

### GET /api/v1/cliente/buscar (SearchByName) — 4 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 15 | `SearchByName_ReturnsMatchingClientes` | Resultados contienen coincidencias |
| 16 | `SearchByName_ReturnsEmpty_WhenNoMatch` | Sin match → Items vacío |
| 17 | `SearchByName_WithPagination_ReturnsCorrectPage` | Paginado respetado |
| 18 | `SearchByName_EmptyTexto_ReturnsBadRequest` | texto vacío → 400 |

### GET /api/v1/cliente/autocomplete (Autocomplete) — 6 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 19 | `Autocomplete_ReturnsMatchingResults` | "Juan" → 2 clientes que inician con Juan |
| 20 | `Autocomplete_ReturnsEmpty_WhenNoMatch` | Sin match → lista vacía |
| 21 | `Autocomplete_ReturnsLimitedResults` | maxResultados=3 de 5 posibles |
| 22 | `Autocomplete_WithoutAuth_Returns401` | Sin token → 401 |
| 23 | `Autocomplete_WithEmptyTexto_Returns400` | texto vacío → 400 |
| 24 | `Autocomplete_ReturnsOrderedResults` | Resultados en orden alfabético |

### POST /api/v1/cliente (Create) — 7 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 19 | `ValidDto_Returns201WithLocationHeader` | 201 + Location → `/api/v1/cliente/{id}` |
| 20 | `ValidDto_ReturnsDtoWithId` | id > 0 |
| 21 | `ValidDto_ReturnsCorrectData` | Campos nombre, correo, teléfono coinciden |
| 22 | `ValidDto_CreatesClienteInDatabase` | GET posterior confirma persistencia |
| 23 | `DuplicateCorreo_Returns400` | Mismo correo → 400 |
| 24 | `EmptyNombre_Returns400` | string vacío → 400 |
| 25 | `NombreTooLong_Returns400` | 101 caracteres → 400 |
| 26 | `InvalidEmail_Returns400` | Formato inválido → 400 |
| 27 | `TrimsNombreCliente` | `"  nombre  "` → `"nombre"` |

### PUT /api/v1/cliente/{id} (Update) — 10 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 28 | `ValidUpdate_Returns204` | 204 NoContent |
| 29 | `ValidUpdate_ChangesData` | GET posterior confirma cambios |
| 30 | `RouteIdMismatch_Returns400` | Route id=1, body id=999 → 400 |
| 31 | `DuplicateCorreo_Returns400` | Cambiar a correo existente → 400 |
| 32 | `NonExistentId_Returns404` | id 9999 → 404 |
| 33 | `StaleRowVersion_Returns409` | RowVersion incorrecto → 409 Conflict |
| 34 | `EmptyNombre_Returns400` | string vacío → 400 |
| 35 | `NombreTooLong_Returns400` | 101 caracteres → 400 |
| 36 | `InvalidEmail_Returns400` | Formato inválido → 400 |
| 37 | `TrimsNombreCliente` | `"  nombre  "` → `"nombre"` |
| 38 | `SelfRename_AllowsSameCorreo` | Mismo correo → 204 (no hay conflicto) |

### DELETE /api/v1/cliente/{id} (Delete) — 6 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 39 | `ValidDelete_Returns200` | 200 OK |
| 40 | `ValidDelete_RemovesClienteFromDatabase` | GET posterior → 404 |
| 41 | `RouteIdMismatch_Returns400` | Route id=1, body id=999 → 400 |
| 42 | `NonExistentId_Returns404` | id 9999 → 404 |
| 43 | `OnlyRemovesTargetCliente` | 2 clientes, elimina 1 → el otro sigue accesible |
| 44 | `StaleRowVersion_Returns409` | RowVersion incorrecto → 409 Conflict |

### Full Lifecycle — 1 prueba

| # | Prueba | Flujo |
|---|--------|-------|
| 45 | `CreateGetUpdateGetDelete_CompleteFlow` | Create (201) → Get (200) → Update (204) → Get (200, datos cambiados) → Delete (200) → Get (404) |

**Total: 49 pruebas**

## 5. Problemas detectados durante la implementación

### 5.1 Contaminación entre pruebas (test pollution)

**Problema**: `IClassFixture<WebApplicationFactory<Program>>` comparte la misma instancia del factory (y por tanto la misma BD InMemory) entre todos los tests del fixture. Los tests que verifican cantidades exactas fallaban porque los tests anteriores ya habían insertado datos.

**Solución**: Implementar `IAsyncLifetime` con limpieza de la tabla `CliCliente` en `InitializeAsync()`, que se ejecuta antes de cada test.

### 5.2 Nombres con guiones bajos rechazados por regex

**Problema**: Los helpers `CreateClienteAsync` usaban nombres con guiones bajos (ej. `"int_cliente_{Guid:N}"`), pero el regex del DTO solo permite letras (con acentos/ñ) y números. Los tests de integración fallaban con 400 BadRequest.

**Solución**: Reemplazar todos los underscores en los nombres de cliente por strings sin guiones bajos, tanto en `CreateClienteAsync` como en las aserciones correspondientes.

### 5.3 Location header con PascalCase

**Problema**: `CreatedAtAction` genera la URL usando `[controller]` que toma el nombre de la clase `ClienteController`, resultando en `/api/v1/Cliente/{id}` (con mayúscula). La aserción buscaba `/api/v1/cliente/` (minúscula).

**Solución**: Corregir el string de búsqueda a `"/api/v1/Cliente/"`.

## 6. Resultado de ejecución

```
Correctas! - Con error: 0, Superado: 49, Omitido: 0, Total: 49, Duración: ~20s
```

Todos los tests se ejecutan con el comando:

```powershell
dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build --filter "FullyQualifiedName~Clientes"
```

## 7. Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `IntegrationTest/Clientes/IntegrationTests.cs` | Clase con 43 tests (CRUD) |
| `IntegrationTest/Clientes/AutocompleteIntegrationTests.cs` | Clase con 6 tests (autocomplete) |
| `UnitTest/Common/JwtTestConfig.cs` | Constantes JWT compartidas |
| `UnitTest/Common/TokenHelper.cs` | Generación de tokens JWT |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs y entidades |
