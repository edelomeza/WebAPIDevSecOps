# Memoria de Pruebas de Integración — Producto

## 1. Introducción

Este documento describe el conjunto de pruebas de integración para el controlador `ProductoController` (endpoints `api/v{version}/producto`). Las pruebas validan el correcto funcionamiento del pipeline completo: enrutamiento, autenticación JWT, autorización por rol, validación de DTOs, lógica de negocio, concurrencia y persistencia.

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

Se implementó `IAsyncLifetime` con un método `InitializeAsync()` que limpia la tabla `ProProducto` antes de cada test. Esto evita contaminación entre pruebas dado que `IClassFixture` comparte la misma instancia de `WebApplicationFactory` (y por tanto la misma BD InMemory) entre todos los tests de la clase.

```csharp
public async Task InitializeAsync()
{
    TokenBlacklist.Clear();
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.ProProducto.RemoveRange(db.ProProducto);
    await db.SaveChangesAsync();
}
```

### Helpers

- **`CreateProductoAsync(uniqueName?)`**: Crea un producto vía POST, luego hace GET para obtener el `RowVersion` (no se devuelve en la respuesta de creación). Retorna `ProProductoDto` completo.
- **`GetProductoAsync(id)`**: GET por id, retorna `ProProductoDto`.
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

### GET /api/v1/producto (GetAll) — 7 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 1 | `EmptyDatabase_ReturnsEmptyPagedResult` | Items vacío, TotalCount=0, PageNumber=1, PageSize=20, TotalPages=0 |
| 2 | `WithProductos_ReturnsDefaultPagination` | Items=3, PageSize=20 (default) |
| 3 | `WithCustomPageSize_ReturnsCorrectSize` | pageSize=5, Items=5 de 15 total |
| 4 | `WithCustomPageNumber_ReturnsCorrectPage` | pageNumber=2, pageSize=10 |
| 5 | `OnLastPage_ReturnsRemainingItems` | Página 3 de 25 registros → Items=5 |
| 6 | `WhenPageExceedsTotal_ReturnsEmpty` | page=10, pageSize=10 → Items vacío |
| 7 | `HasCacheControlHeader` | `Cache-Control: no-store` |

### GET /api/v1/producto/buscar (SearchByName) — 4 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 8 | `SearchByName_ReturnsMatchingProductos` | Resultados contienen coincidencias |
| 9 | `SearchByName_ReturnsEmpty_WhenNoMatch` | Sin match → Items vacío |
| 10 | `SearchByName_WithPagination_ReturnsCorrectPage` | Paginado respetado |
| 11 | `SearchByName_EmptyTexto_ReturnsBadRequest` | texto vacío → 400 |

### GET /api/v1/producto/{id} (GetById) — 5 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 12 | `ExistingProducto_ReturnsProducto` | 200, datos correctos |
| 13 | `NonExistentId_ReturnsNotFound` | 404 (id 9999) |
| 14 | `NegativeId_ReturnsNotFound` | 404 (id -1) |
| 15 | `ZeroId_ReturnsNotFound` | 404 (id 0) |
| 16 | `MultipleProductos_ReturnsCorrectProducto` | Entre 3 productos, obtiene el del medio |

### POST /api/v1/producto (Create) — 8 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 17 | `ValidDto_Returns201WithLocationHeader` | 201 + Location → `/api/v1/Producto/{id}` |
| 18 | `ValidDto_ReturnsDtoWithId` | id > 0 |
| 19 | `ValidDto_ReturnsCorrectData` | Campos nombre, existencia, precio coinciden |
| 20 | `ValidDto_CreatesProductoInDatabase` | GET posterior confirma persistencia |
| 21 | `EmptyNombre_Returns400` | string vacío → 400 |
| 22 | `NombreTooLong_Returns400` | 51 caracteres → 400 |
| 23 | `TrimsNombreProducto` | `"  nombre  "` → `"nombre"` |
| 24 | `AllowsDuplicateNombre` | Mismo nombre → 201 (nombres no únicos) |

### PUT /api/v1/producto/{id} (Update) — 7 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 25 | `ValidUpdate_Returns204` | 204 NoContent |
| 26 | `ValidUpdate_ChangesData` | GET posterior confirma cambios |
| 27 | `RouteIdMismatch_Returns400` | Route id=1, body id=999 → 400 |
| 28 | `NonExistentId_Returns404` | id 9999 → 404 |
| 29 | `StaleRowVersion_Returns409` | RowVersion incorrecto → 409 Conflict |
| 30 | `EmptyNombre_Returns400` | string vacío → 400 |
| 31 | `NombreTooLong_Returns400` | 51 caracteres → 400 |

### DELETE /api/v1/producto/{id} (Delete) — 6 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 32 | `ValidDelete_Returns200` | 200 OK |
| 33 | `ValidDelete_RemovesProductoFromDatabase` | GET posterior → 404 |
| 34 | `RouteIdMismatch_Returns400` | Route id=1, body id=999 → 400 |
| 35 | `NonExistentId_Returns404` | id 9999 → 404 |
| 36 | `OnlyRemovesTargetProducto` | 2 productos, elimina 1 → el otro sigue accesible |
| 37 | `StaleRowVersion_Returns409` | RowVersion incorrecto → 409 Conflict |

### Full Lifecycle — 1 prueba

| # | Prueba | Flujo |
|---|--------|-------|
| 38 | `FullLifecycle_CreateGetUpdateGetDelete_CompleteFlow` | Create (201) → Get (200) → Update (204) → Get (200, datos cambiados) → Delete (200) → Get (404) |

**Total: 38 pruebas**

## 5. Problemas detectados durante la implementación

### 5.1 Contaminación entre pruebas (test pollution)

**Problema**: `IClassFixture<WebApplicationFactory<Program>>` comparte la misma instancia del factory (y por tanto la misma BD InMemory) entre todos los tests del fixture. Los tests que verifican cantidades exactas fallaban porque los tests anteriores ya habían insertado datos.

**Solución**: Implementar `IAsyncLifetime` con limpieza de la tabla `ProProducto` en `InitializeAsync()`, que se ejecuta antes de cada test.

### 5.2 Location header con PascalCase

**Problema**: `CreatedAtAction` genera la URL usando `[controller]` que toma el nombre de la clase `ProductoController`, resultando en `/api/v1/Producto/{id}` (con mayúscula). La aserción buscaba `/api/v1/producto/` (minúscula).

**Solución**: Corregir el string de búsqueda a `"/api/v1/Producto/"`.

## 6. Resultado de ejecución

```
Correctas! - Con error: 0, Superado: 38, Omitido: 0, Total: 38, Duración: ~16s
```

Todos los tests se ejecutan con el comando:

```powershell
dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build --filter "FullyQualifiedName~Producto"
```

## 7. Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `IntegrationTest/Producto/IntegrationTests.cs` | Clase con 38 tests |
| `UnitTest/Common/JwtTestConfig.cs` | Constantes JWT compartidas |
| `UnitTest/Common/TokenHelper.cs` | Generación de tokens JWT |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs y entidades |
