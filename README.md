# WebAPIDevSecOps

> **Este proyecto aplica buenas prácticas de DevSecOps** en todo su ciclo de vida: análisis estático (SAST), pruebas dinámicas (DAST), escaneo de dependencias, generación de SBOM, escaneo de contenedores, pruebas de seguridad automatizadas y un pipeline CI/CD que integra seguridad en cada etapa.

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-green)

API REST segura en ASP.NET Core 10 con autenticación JWT, hashing Argon2id, rate limiting y SQL Server, con un pipeline CI/CD completo orientado a DevSecOps.

---

## Stack tecnológico

| Categoría | Tecnologías |
|---|---|
| **Runtime** | .NET 10, ASP.NET Core 10 |
| **Base de datos** | SQL Server (EF Core) / InMemory (desarrollo y tests) |
| **Autenticación** | JWT Bearer (HMAC-SHA256) |
| **Hashing** | Argon2id (Konscious) + BCrypt fallback |
| **API Docs** | OpenAPI + Scalar UI |
| **Logging** | Serilog (archivos JSON + consola) |
| **Resiliencia** | Polly (circuit breaker) |
| **Validación** | FluentValidation |

---

## Seguridad implementada

| Feature | Detalle |
|---|---|
| **JWT** | Token 60 min, HMAC-SHA256, clock skew zero, validación de issuer/audience |
| **Password** | Argon2id (64 MB de memoria, 3 iteraciones) + BCrypt como fallback |
| **Rate Limiting** | Global 1000 request/min, Login 5 requests/5min |
| **Token Blacklist** | Logout invalida el token inmediatamente (caché en memoria) |
| **Security Headers** | CSP, HSTS, X-Frame-Options, X-Content-Type-Options, X-XSS-Protection, Referrer-Policy, Permissions-Policy |
| **Exception Handling** | Middleware que mapea excepciones a códigos HTTP correctos (409, 404, 403, 400, 500) |
| **Account Lockout** | 5 intentos fallidos de login bloquean la cuenta por 15 minutos |
| **Request Timeout** | 60 segundos configurable |
| **Kestrel Limits** | Máx. 1000 conexiones concurrentes, 1 MB de body |
| **CORS** | Un solo origen autorizado (https://localhost:5097) |
| **Anti-enumeration** | Fake hash para evitar timing attacks en login |

---

## Setup rápido

```powershell
# 1. Copiar template de configuración
cp WebAPIDevSecOps/appsettings.Example.json WebAPIDevSecOps/appsettings.json

# 2. Editar appsettings.json con tu conexión local
#    O usar base de datos en memoria para desarrollo rápido:
#    Agregar "UseInMemoryDatabase": true en appsettings.json

# 3. Restaurar dependencias y ejecutar
dotnet restore
dotnet run --project WebAPIDevSecOps/WebAPIDevSecOps.csproj
```

**URLs de desarrollo:**  
- `http://localhost:5196`  
- `https://localhost:7227`  
- Documentación API: `/scalar`

---

## Endpoints de la API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `POST` | `/api/v1/login/login` | No | Autenticación de usuario |
| `POST` | `/api/v1/logout/logout` | No | Invalidar token JWT |
| `GET` | `/api/v1/usuario` | Admin | Lista paginada de usuarios |
| `GET` | `/api/v1/usuario/{id}` | Admin | Obtener usuario por ID |
| `POST` | `/api/v1/usuario` | Admin | Crear nuevo usuario |
| `PUT` | `/api/v1/usuario/{id}` | Admin | Actualizar usuario existente |
| `DELETE` | `/api/v1/usuario/{id}` | Admin | Eliminar usuario |
| `GET` | `/api/v1/tipoempleado` | Auth | Catálogo de tipos de empleado |
| `GET` | `/api/v1/tipoempleado/{id}` | Auth | Tipo de empleado por ID |
| `GET` | `/health` | No | Health checks completos |
| `GET` | `/health/ready` | No | Health check solo base de datos |

---

## Estructura del proyecto

```
WebAPIDevSecOps/
├── WebAPIDevSecOps/            # API principal
│   ├── Program.cs               # Entrypoint y pipeline de middleware
│   ├── appsettings.json        # Configuración local (no versionado)
│   ├── appsettings.Example.json # Template de configuración (versionado)
│   ├── Controllers/             # Endpoints de la API
│   ├── Services/                # Lógica de negocio
│   ├── Middleware/              # Exception, audit logging, timeout
│   ├── Context/                 # EF Core DbContext
│   ├── Models/                  # Entidades de base de datos
│   ├── Dto/                     # Request/Response + Validadores
│   └── Migrations/              # Migraciones de EF Core
├── UnitTest/                    # Tests unitarios (xUnit)
├── IntegrationTest/             # Tests de integración (WebApplicationFactory)
└── SecurityTest/                # Tests de seguridad
```

---

## CI/CD Pipeline

```
push → restore → build → unit tests → integration tests → security tests
       → vuln check → SBOM → docker build → Trivy scan → DAST (ZAP)
       → SonarCloud → Semgrep (solo PR)
```

| Etapa | Herramienta | ¿Qué verifica? |
|---|---|---|
| **Unit Tests** | xUnit + Moq + FluentAssertions | Lógica de negocio (56 tests) |
| **Integration Tests** | WebApplicationFactory | API completa contra base de datos (52 tests) |
| **Security Tests** | WebApplicationFactory | SQLi, XSS, JWT, rate limiting, headers (25 tests) |
| **Vulnerable Dependencies** | `dotnet list package --vulnerable` | Paquetes NuGet con CVEs conocidos |
| **SBOM** | CycloneDX | Genera inventario de dependencias en formato CycloneDX |
| **Container Scan** | Trivy | Escanea la imagen Docker; HIGH/CRITICAL bloquea el push |
| **DAST** | OWASP ZAP | Ataques dinámicos a la API usando OpenAPI spec |
| **SAST** | SonarCloud + Semgrep | Análisis estático de código con reglas personalizadas |

---

## Tests

```powershell
# Unitarios
dotnet test UnitTest/UnitTest.csproj

# Integración
dotnet test IntegrationTest/IntegrationTest.csproj

# Seguridad
dotnet test SecurityTest/SecurityTest.csproj

# Todos
dotnet test UnitTest/UnitTest.csproj
dotnet test IntegrationTest/IntegrationTest.csproj
dotnet test SecurityTest/SecurityTest.csproj
```

---

## Configuración

Las secciones clave de `appsettings.json`:

| Sección | Propósito |
|---|---|
| `ConnectionStrings:DefaultConnection` | Conexión a SQL Server |
| `Jwt` | Key (mín. 32 bytes), Issuer, Audience |
| `PasswordHashing` | MemorySize, Iterations, DegreeOfParallelism |
| `Resilience` | Parámetros del circuit breaker |
| `RequestTimeoutSeconds` | Timeout global de requests |

Las credenciales de base de datos pueden sobrescribirse vía variables de entorno: `DB_USER` y `DB_PASSWORD`.

---

## Licencia

MIT
