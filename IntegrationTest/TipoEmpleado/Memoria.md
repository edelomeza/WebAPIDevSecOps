# Memoria de Pruebas de Integración — TipoEmpleado

## 1. Introducción

Este documento describe el conjunto de pruebas de integración para el controlador `TipoEmpleadoController` (endpoints `api/v{version}/tipoempleado`). Es un endpoint de solo lectura (GetAll + GetById) que expone los valores del catálogo de tipos de empleado.

Tecnologías utilizadas:
- **xUnit** + **FluentAssertions**
- **WebApplicationFactory\<Program\>** para hostear la aplicación en memoria
- **EF Core InMemory** como sustituto de SQL Server
- **JwtTestConfig** (clase compartida en `UnitTest/Common/`)

## 2. Arquitectura de pruebas

```
WebApplicationFactory<Program>
  └─ WithWebHostBuilder
       ├─ UseSetting("UseInMemoryDatabase", "true")
       ├─ UseSetting("Jwt:Key", ...)
       └─ UseSetting("Jwt:Issuer"/"Jwt:Audience", ...)
  └─ CreateClient() → HttpClient autenticado
```

### Aislamiento entre pruebas

Se implementó `IAsyncLifetime` limpiando `EmpCatTipoEmpleado` en `InitializeAsync()`.

## 3. Escenarios cubiertos

### GET /api/v1/tipoempleado (GetAll) — 4 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 1 | `EmptyDatabase_ReturnsEmptyPagedResult` | Items vacío, TotalCount=0 |
| 2 | `WithData_ReturnsItems` | Items=2, TotalCount=2 |
| 3 | `Pagination_Works` | pageSize=2, pageNumber=2 |
| 4 | `NoStoreCacheHeader` | `Cache-Control: no-store` |

### GET /api/v1/tipoempleado/{id} (GetById) — 4 pruebas

| # | Prueba | Verificación |
|---|--------|-------------|
| 5 | `Existing_ReturnsDto` | 200, strValor correcto |
| 6 | `NonExistent_ReturnsNotFound` | 404 (id 9999) |
| 7 | `NegativeId_ReturnsNotFound` | 404 (id -1) |
| 8 | `ZeroId_ReturnsNotFound` | 404 (id 0) |

**Total: 8 pruebas**

## 4. Resultado de ejecución

```
Correctas! - Con error: 0, Superado: 8, Omitido: 0, Total: 8, Duración: ~4s
```

```powershell
dotnet test IntegrationTest/IntegrationTest.csproj -c Release --no-build --filter "FullyQualifiedName~TipoEmpleado"
```

## 5. Archivos involucrados

| Archivo | Rol |
|---------|-----|
| `IntegrationTest/TipoEmpleado/IntegrationTests.cs` | Clase con 8 tests |
| `UnitTest/Common/JwtTestConfig.cs` | Constantes JWT compartidas |
| `UnitTest/Common/TokenHelper.cs` | Generación de tokens JWT |
