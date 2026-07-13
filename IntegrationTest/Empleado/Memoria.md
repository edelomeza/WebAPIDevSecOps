# Memoria de Pruebas de Integración — Empleado

## 1. Introducción

Este documento describe el conjunto de pruebas de integración para el controlador `EmpleadoController` (endpoints `api/v{version}/empleado`). Las pruebas validan el correcto funcionamiento del pipeline completo: enrutamiento, autenticación JWT, autorización por rol, validación de DTOs, lógica de negocio, concurrencia y persistencia.

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

Se implementó `IAsyncLifetime` con un método `InitializeAsync()` que limpia la tabla `EmpEmpleado` antes de cada test.

```csharp
public async Task InitializeAsync()
{
    TokenBlacklist.Clear();
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.EmpEmpleado.RemoveRange(db.EmpEmpleado);
    await db.SaveChangesAsync();
}
```

### Helpers

- **`CreateEmpleadoAsync(uniqueName?)`**: Crea un empleado vía POST, luego hace GET para obtener el `RowVersion`. Retorna `EmpEmpleadoDto` completo.
- **`AdminToken`**: Token JWT con rol `Admin`.

## 3. Clase utilitaria compartida

Se reutiliza `UnitTest/Common/JwtTestConfig.cs` para las constantes JWT.

## 4. Escenarios cubiertos

### GET /api/v1/empleado (GetAll) — 3 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 1 | `EmptyDatabase_ReturnsEmptyPagedResult` | Items vacío, TotalCount=0, PageNumber=1, PageSize=20 |
| 2 | `WithUsers_ReturnsDefaultPagination` | Items=2, PageSize=20 |
| 3 | `WithCustomPagination_ReturnsCorrectSize` | pageSize=2, pageNumber=2 |

### GET /api/v1/empleado/{id} (GetById) — 2 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 4 | `ExistingUser_ReturnsUser` | 200, datos correctos |
| 5 | `NonExistentId_ReturnsNotFound` | 404 (id 9999) |

### GET /api/v1/empleado/buscar (Search) — 2 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 6 | `Search_ByText_ReturnsMatches` | Búsqueda por texto parcial |
| 7 | `Search_WithPagination_Works` | Paginado respetado |

### POST /api/v1/empleado (Create) — 5 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 8 | `ValidDto_Returns201WithLocation` | 201 + Location → `/api/v1/Empleado/{id}` |
| 9 | `ValidDto_ReturnsDtoWithId` | id > 0 |
| 10 | `DuplicateCURP_Returns400` | Misma CURP → 400 |
| 11 | `EmptyNombre_Returns400` | string vacío → 400 |
| 12 | `NombreTooLong_Returns400` | 51 caracteres → 400 |

### PUT /api/v1/empleado/{id} (Update) — 4 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 13 | `ValidUpdate_Returns204` | 204 NoContent |
| 14 | `RouteIdMismatch_Returns400` | Route id=1, body id=999 → 400 |
| 15 | `NonExistentId_Returns404` | id 9999 → 404 |
| 16 | `StaleRowVersion_Returns409` | RowVersion incorrecto → 409 Conflict |

### DELETE /api/v1/empleado/{id} (Delete) — 5 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 17 | `ValidDelete_Returns200` | 200 OK |
| 18 | `RemovesFromDatabase` | GET posterior → 404 |
| 19 | `RouteIdMismatch_Returns400` | Route id=1, body id=999 → 400 |
| 20 | `NonExistentId_Returns404` | id 9999 → 404 |
| 21 | `StaleRowVersion_Returns409` | RowVersion incorrecto → 409 Conflict |

### Full Lifecycle — 1 prueba

| # | Prueba | Flujo |
|---|--------|-------|
| 22 | `FullLifecycle_CreateGetUpdateGetDelete_CompleteFlow` | Create (201) → Get (200) → Update (204) → Get (200) → Delete (200) → Get (404) |

**Total: 22 pruebas**

## 5. Problemas detectados durante la implementación

### 5.1 CURP único con nulls permitidos

La validación de CURP duplicado debe ignorar registros con `strCURP == null` para permitir múltiples empleados sin CURP. Se implementó con filtro adicional en LINQ.

### 5.2 Nombres con guiones bajos en tests

Los helpers usaban nombres como `"search_juan"` que son válidos en InMemory pero el regex del DTO (`^[a-zA-Z0-9áéíóúÁÉÓÍÚñÑ ]+$`) los rechazaría si se aplicara en creación. Los tests de creación usan CURP válidos predefinidos para evitar este problema.

## 6. Resultado de ejecución

```
Correctas! - Con error: 0, Superado: 22, Omitido: 0, Total: 22, Duración: ~12s
```

```powershell
dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build --filter "FullyQualifiedName~Empleado"
```

## 7. Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `IntegrationTest/Empleado/IntegrationTests.cs` | Clase con 22 tests |
| `UnitTest/Common/JwtTestConfig.cs` | Constantes JWT compartidas |
| `UnitTest/Common/TokenHelper.cs` | Generación de tokens JWT |
| `UnitTest/Common/TestDataFactory.cs` | Fábrica de DTOs y entidades |
