# Memoria: Docker, GitHub CI/CD y Test Isolation

## Fecha: 27 de junio de 2026

## Contexto

El proyecto WebAPIDevSecOps tuvo problemas al subir a GitHub. El Copilot de GitHub diagnosticó los problemas y sugirió correcciones en el Dockerfile y el workflow de CI/CD. Posteriormente, el pipeline falló en Integration Tests debido a contaminación de estado entre tests.

## Problemas encontrados y corregidos

### Dockerfile

| Problema | Antes | Después |
|----------|-------|---------|
| `USER $APP_UID` sin definir | Variable inexistente en línea 32 | `ARG APP_UID=1000` + `useradd` + `chown /app` |
| Etapa `test` muerta | 3 `RUN dotnet test` que nunca se ejecutaban | Eliminada (los tests corren en CI) |

### Workflow CI/CD (.github/workflows/ci-cd.yml)

| # | Problema | Solución |
|---|----------|----------|
| 1 | Sin `permissions` explícitos | Agregado `contents: read`, `packages: write`, `security-events: write` |
| 2 | Sin `concurrency` | Agregado grupo `ci-${{ github.ref }}` con `cancel-in-progress: true` |
| 3 | Tags Docker inconsistentes | Unificado a `${{ github.repository }}:${{ github.sha }}` (determinista) |
| 4 | Cosign/Trivy con multi-tag reference | Usan `IMAGE_REF` con el tag único |
| 5 | `secrets.SONAR_TOKEN != ''` no funciona en `if` | Eliminado (los secrets no se evalúan en condiciones de jobs) |
| 6 | Actions con `@master` sin versión fija | Trivy `@v0.28.0`, SonarQualityGate `@v1.1.1` |
| 7 | Sin `persist-credentials: false` | Agregado en todos los checkout (5 jobs) |
| 8 | Sin `timeout-minutes` | Agregado en todos los jobs (10-30 min) |

## Fase 2: Corrección Semgrep

| Problema | Solución |
|----------|----------|
| Opción `--error` no válida en Semgrep | Eliminada del comando `semgrep ci` |

- **Commit**: `7a3556e`

## Fase 3: Aislamiento de Integration Tests

### Problemas encontrados

| # | Problema | Severidad |
|---|----------|-----------|
| 1 | InMemory DB hardcodeada `"AppDb"` compartida entre todas las clases de test | CRÍTICO |
| 2 | TokenBlacklist estática sin método `Clear()` | CRÍTICO |
| 3 | Sin control de paralelismo xUnit | ALTO |
| 4 | 3 de 4 clases sin limpieza de estado (IAsyncLifetime) | MEDIO-ALTO |
| 5 | Login tests sin `ConnectionStrings:DefaultConnection` | MEDIO |

### Soluciones aplicadas

| Archivo | Cambio |
|---------|--------|
| `IntegrationTest/AssemblyInfo.cs` | **CREAR** - `[assembly: CollectionBehavior(DisableTestParallelization = true)]` |
| `SecurityTest/AssemblyInfo.cs` | **CREAR** - `[assembly: CollectionBehavior(DisableTestParallelization = true)]` |
| `TokenBlacklist.cs` | Agregado `public static void Clear()` |
| `Program.cs` | DB name configurable: `builder.Configuration["InMemoryDatabaseName"] ?? "AppDb"` |
| `IntegrationTests.cs` | DB única GUID + `TokenBlacklist.Clear()` en InitializeAsync |
| `LoginIntegrationTest.cs` | IAsyncLifetime + DB única + ConnectionString + `TokenBlacklist.Clear()` |
| `SecurityTests.cs` | IAsyncLifetime + DB única + `TokenBlacklist.Clear()` |
| `LoginSecurityTest.cs` | IAsyncLifetime + DB única + ConnectionString + `TokenBlacklist.Clear()` |

### Tests locales (133 total)

| Suite | Tests | Estado |
|-------|-------|--------|
| Integration Tests | 52 | ✅ Todos pasan |
| Security Tests | 25 | ✅ Todos pasan |
| Unit Tests | 56 | ✅ Todos pasan |

- **Commit**: `086a80e`

## Estado del PR

- **PR #10**: "Docker y Pipeliene mejorado"
- **Rama**: `fix/ci-dockerfile-and-signing` → `main`
- **Commits**: 3 commits (`7ee78d9`, `7a3556e`, `086a80e`)
- **Archivos modificados**: `Dockerfile`, `ci-cd.yml`, `Program.cs`, `TokenBlacklist.cs`, + 4 archivos de test

### Checks del PR

| Job | Estado | Notas |
|-----|--------|-------|
| Build & Test | Pendiente re-ejecución | Después de los 3 commits |
| Semgrep SAST | Corregido | Eliminada opción `--error` inválida |
| docker-build | No ejecuta en PRs | Solo corre en push a `main` |
| fuzz | No ejecuta en PRs | Solo corre en push a `main` |
| dast | No ejecuta en PRs | Solo corre en push a `main` |

## Próximos pasos al hacer merge

1. Los jobs `docker-build`, `fuzz` y `dast` se ejecutarán automáticamente
2. `sonarcloud` y `semgrep` correrán según la rama destino
3. Verificar que el PR #10 pase todos los checks de CI

## Lecciones aprendidas

1. **No ejecutar tests en el Dockerfile**: Los tests ya corren en CI, duplicarlos en el build innecesariamente.
2. **Usar tags deterministas**: `repo:sha` es más robusto que tags dinámicos de metadata-action.
3. **Los secrets no se evalúan en `if` de jobs**: GitHub Actions no permite acceder a secrets en condiciones de job.
4. **Fijar versiones de actions**: Usar `@v0.28.0` en vez de `@master` para estabilidad.
5. **Seguridad en checkout**: `persist-credentials: false` evita que el token quede en git config.
6. **Aislar InMemory DB por test class**: Nunca hardcodear el nombre de BD InMemory; usar GUIDs únicos.
7. **Limpiar estado estático entre tests**: TokenBlacklist u otros estados estáticos deben tener `Clear()` y llamarse en `InitializeAsync`.
8. **Deshabilitar paralelismo en tests de integración**: Cuando se usa InMemory DB o estado estático, `DisableTestParallelization = true` previene condiciones de carrera.
9. **Siempre setear `ConnectionStrings:DefaultConnection`**: Incluso cuando se usa InMemory, Program.cs lee la cadena de conexión antes del check de `UseInMemoryDatabase`.
