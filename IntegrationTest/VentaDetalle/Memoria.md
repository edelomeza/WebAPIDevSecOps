# Memoria de Pruebas de Integración — VentaDetalle

## 1. Introducción

Este documento describe el conjunto de pruebas de integración para el controlador `VentaDetalleController` (endpoints `api/v{version}/ventadetalle`). Las pruebas validan el correcto funcionamiento del pipeline completo: enrutamiento, autenticación JWT, autorización por rol, validación de DTOs, lógica de negocio (cálculo de `decTotalVenta`), búsqueda de productos para autocomplete y persistencia.

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
       ├─ UseSetting("InMemoryDatabaseName", "VentaDetalleTestDb_{Guid}") → BD única por clase
       ├─ UseSetting("Jwt:Key", ...)                   → clave JWT de prueba
       └─ UseSetting("Jwt:Issuer"/"Jwt:Audience", ...)
  └─ CreateClient() → HttpClient autenticado
```

### Aislamiento entre pruebas

Se implementó `IAsyncLifetime` con un método `InitializeAsync()` que limpia la tabla `VenVentaDetalle` y `TokenBlacklist` antes de cada test. Esto evita contaminación entre pruebas dado que `IClassFixture` comparte la misma instancia de `WebApplicationFactory`.

```csharp
public async Task InitializeAsync()
{
    TokenBlacklist.Clear();
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Set<VenVentaDetalle>().RemoveRange(db.Set<VenVentaDetalle>());
    await db.SaveChangesAsync();
}
```

### Helpers

- **`CreateDetalleAsync()`**: Crea venta y producto vía POST, luego crea un detalle y lo retorna completo vía GET.
- **`AdminToken`**: Token JWT con rol `Admin` (requerido por la policy `AdminOnly` en todos los endpoints).
- **`SeedDependenciesAsync()`**: Crea CliCliente + SegUsuario + VenCatEstado + ProProducto + VenVenta, retorna IDs.

## 3. Clase utilitaria compartida

Se reutiliza `UnitTest/Common/JwtTestConfig.cs` para centralizar las constantes JWT:

```csharp
public static class JwtTestConfig
{
    public const string Key = "01123581321345589144233377610987";
    public const string Issuer = "edelmeza.com";
    public const string Audience = "edelmeza.com";
    public static string AdminToken => TokenHelper.GenerateValidToken(Key, Issuer, Audience);
}
```

## 4. Escenarios cubiertos

### GET /api/v1/ventadetalle (GetAll) — 6 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 1 | `EmptyDatabase_ReturnsEmptyPagedResult` | Items vacío, TotalCount=0, PageNumber=1, PageSize=20, TotalPages=0 |
| 2 | `WithDetalles_ReturnsDefaultPagination` | Items=2, PageSize=20 (default) |
| 3 | `WithCustomPageSize_ReturnsCorrectSize` | pageSize=5, Items=5 de 15 total |
| 4 | `WithCustomPageNumber_ReturnsCorrectPage` | pageNumber=2, pageSize=10 |
| 5 | `OnLastPage_ReturnsRemainingItems` | Página 3 de 25 registros → Items=5 |
| 6 | `HasCacheControlHeader` | `Cache-Control: no-store` |

### GET /api/v1/ventadetalle/{id} (GetById) — 3 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 7 | `ExistingDetalle_ReturnsDetalle` | 200, datos correctos (intPiezaVenta, decTotalVenta) |
| 8 | `NonExistentId_ReturnsNotFound` | 404 (id 9999) |
| 9 | `NegativeId_ReturnsNotFound` | 404 (id -1) |
| 10 | `ZeroId_ReturnsNotFound` | 404 (id 0) |

### POST /api/v1/ventadetalle (Create) — 7 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 11 | `ValidDto_Returns201WithLocationHeader` | 201 + Location → `/api/v1/VentaDetalle/{id}` |
| 12 | `ValidDto_ReturnsDtoWithId` | id > 0 |
| 13 | `ValidDto_ReturnsCorrectData` | Campos idVenVenta, idProProducto, intPiezaVenta, decTotalVenta coinciden |
| 14 | `ValidDto_CreatesDetalleInDatabase` | GET posterior confirma persistencia |
| 15 | `NonExistentVenta_Returns400` | idVenVenta=9999 → 400 |
| 16 | `NonExistentProducto_Returns400` | idProProducto=9999 → 400 |

### GET /api/v1/ventadetalle/buscarproducto (BuscarProducto) — 2 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 17 | `BuscarProducto_ReturnsMatchingProductos` | Búsqueda parcial retorna resultados con id + texto formateado |
| 18 | `BuscarProducto_EmptyTexto_Returns400` | texto vacío → 400 |

**Total: 18 pruebas**

## 5. Problemas detectados durante la implementación

### 5.1 Cadena de dependencias para Create

**Problema**: Para crear un detalle se requieren `VenVenta` y `ProProducto` existentes, que a su vez requieren `CliCliente` + `SegUsuario` + `VenCatEstado`. La creación de un detalle implica preparar toda la cadena de FKs.

**Solución**: Helper `SeedDependenciesAsync()` que crea toda la cadena y `CreateDetalleAsync()` que la reutiliza. El helper `CreateDetalleAsync` con parámetros opcionales permite compartir dependencias entre múltiples detalles.

### 5.2 Cálculo de decTotalVenta del lado del servidor

**Problema**: El DTO de creación `VenVentaDetalleCreateDto` no incluye `decTotalVenta` — este campo se calcula en el service como `intPiezaVenta * decPrecio`. Las pruebas deben conocer el precio del producto seedeado para verificar el cálculo.

**Solución**: `SeedDependenciesAsync()` retorna el precio del producto. El test usa ese precio para el assertion: `decTotalVenta.Should().Be(piezas * precio)`.

### 5.3 Location header con PascalCase

**Problema**: `CreatedAtAction` genera la URL con `[controller]` → `VentaDetalleController` → `/api/v1/VentaDetalle/{id}` (con mayúscula).

**Solución**: Corregir el string de búsqueda a `"/api/v1/VentaDetalle/"`.

## 6. Resultado de ejecución

```
Correctas! - Con error: 0, Superado: 18, Omitido: 0, Total: 18, Duración: ~7s
```

Todos los tests se ejecutan con el comando:

```powershell
dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build --filter "FullyQualifiedName~VentaDetalle"
```

## 7. Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `IntegrationTest/VentaDetalle/IntegrationTests.cs` | Clase con 18 tests |
| `UnitTest/Common/JwtTestConfig.cs` | Constantes JWT compartidas |
| `UnitTest/Common/TokenHelper.cs` | Generación de tokens JWT |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs y entidades |
