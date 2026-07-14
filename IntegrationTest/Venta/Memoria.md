# Memoria de Pruebas de Integración — Venta

## 1. Introducción

Este documento describe el conjunto de pruebas de integración para el controlador `VentaController` (endpoints `api/v{version}/venta`). Las pruebas validan el correcto funcionamiento del pipeline completo: enrutamiento, autenticación JWT, autorización por rol, validación de DTOs, lógica de negocio (generación de clave única, validación de FKs), búsqueda con filtros combinados y persistencia.

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
       ├─ UseSetting("InMemoryDatabaseName", "IntegrationTestVentaDb_{Guid}") → BD única por clase
       ├─ UseSetting("Jwt:Key", ...)                   → clave JWT de prueba
       └─ UseSetting("Jwt:Issuer"/"Jwt:Audience", ...)
  └─ CreateClient() → HttpClient autenticado
```

### Aislamiento entre pruebas

Se implementó `IAsyncLifetime` con un método `InitializeAsync()` que limpia la tabla `VenVenta` y `TokenBlacklist` antes de cada test. Esto evita contaminación entre pruebas dado que `IClassFixture` comparte la misma instancia de `WebApplicationFactory`.

```csharp
public async Task InitializeAsync()
{
    TokenBlacklist.Clear();
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.VenVenta.RemoveRange(db.VenVenta);
    await db.SaveChangesAsync();
}
```

### Helpers

- **`CreateVentaAsync()`**: Crea cliente y usuario vía POST, luego crea una venta y la retorna completa vía GET.
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
}
```

## 4. Escenarios cubiertos

### GET /api/v1/venta (GetAll) — 6 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 1 | `EmptyDatabase_ReturnsEmptyPagedResult` | Items vacío, TotalCount=0, PageNumber=1, PageSize=20, TotalPages=0 |
| 2 | `WithVentas_ReturnsDefaultPagination` | Items=3, PageSize=20 (default) |
| 3 | `WithCustomPageSize_ReturnsCorrectSize` | pageSize=5, Items=5 de 15 total |
| 4 | `WithCustomPageNumber_ReturnsCorrectPage` | pageNumber=2, pageSize=10 |
| 5 | `OnLastPage_ReturnsRemainingItems` | Página 3 de 25 registros → Items=5 |
| 6 | `HasCacheControlHeader` | `Cache-Control: no-store` |

### GET /api/v1/venta/{id} (GetById) — 6 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 7 | `ExistingVenta_ReturnsVenta` | 200, datos correctos (clave, cliente, usuario, estado, fecha) |
| 8 | `NonExistentId_ReturnsNotFound` | 404 (id 9999) |
| 9 | `NegativeId_ReturnsNotFound` | 404 (id -1) |
| 10 | `ZeroId_ReturnsNotFound` | 404 (id 0) |
| 11 | `MultipleVentas_ReturnsCorrectVenta` | Entre 3 ventas, obtiene la del medio |

### POST /api/v1/venta (Create) — 5 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 12 | `ValidDto_Returns201WithLocationHeader` | 201 + Location → `/api/v1/Venta/{id}` |
| 13 | `ValidDto_ReturnsDtoWithId` | id > 0 |
| 14 | `ValidDto_ReturnsCorrectData` | Campos idCliCliente, idSegUsuario, strClaveVenta coinciden |
| 15 | `ValidDto_CreatesVentaInDatabase` | GET posterior confirma persistencia |
| 16 | `NonExistentCliente_Returns400` | idCliCliente=9999 → 400 |
| 17 | `NonExistentUsuario_Returns400` | idSegUsuario=9999 → 400 |

### GET /api/v1/venta/buscar (Search) — 2 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 18 | `ByClaveVenta_ReturnsMatching` | Búsqueda parcial por clave retorna resultados |
| 19 | `WithPagination_ReturnsCorrectPage` | Paginado respetado en búsqueda |

**Total: 19 pruebas**

## 5. Problemas detectados durante la implementación

### 5.1 Contaminación entre pruebas (test pollution)

**Problema**: `IClassFixture<WebApplicationFactory<Program>>` comparte la misma instancia del factory entre todos los tests. Los tests que verifican cantidades exactas fallaban por datos insertados en tests anteriores.

**Solución**: Implementar `IAsyncLifetime` con limpieza de la tabla `VenVenta` y `TokenBlacklist` en `InitializeAsync()`.

### 5.2 Dependencia de FK para Create

**Problema**: Para crear una venta se requieren `CliCliente` y `SegUsuario` existentes en BD. Los helpers de test deben crear estos registros antes de cada venta, lo que incrementa la complejidad del setup.

**Solución**: Helper `CreateVentaAsync()` que crea cliente y usuario vía POST antes de crear la venta, todo en una misma transacción de test.

### 5.3 Location header con PascalCase

**Problema**: `CreatedAtAction` genera la URL con `[controller]` → `VentaController` → `/api/v1/Venta/{id}` (mayúscula).

**Solución**: Corregir el string de búsqueda a `"/api/v1/Venta/"`.

## 6. Resultado de ejecución

```
Correctas! - Con error: 0, Superado: 19, Omitido: 0, Total: 19, Duración: ~30s
```

Todos los tests se ejecutan con el comando:

```powershell
dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build --filter "FullyQualifiedName~Venta"
```

## 7. Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `IntegrationTest/Venta/IntegrationTests.cs` | Clase con 19 tests |
| `UnitTest/Common/JwtTestConfig.cs` | Constantes JWT compartidas |
| `UnitTest/Common/TokenHelper.cs` | Generación de tokens JWT |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs y entidades |

## 8. Lecciones aprendidas (extensión a VentaDetalle)

1. **Limpieza de tablas específicas**: Para VentaDetalle se usa `db.Set<VenVentaDetalle>().RemoveRange(...)` en lugar de `db.VenVenta.RemoveRange(...)`. Cada módulo debe limpiar su propia tabla.

2. **GUID para nombres únicos**: En tests de integración, los nombres de productos y entidades FK se generan con `Guid.NewGuid()` para evitar colisiones cuando múltiples pruebas comparten la misma BD InMemory.

3. **Location header consistente**: Todos los controladores (`VentaController`, `VentaDetalleController`, etc.) generan Location con el nombre PascalCase de la clase: `/api/v1/VentaDetalle/{id}`.
