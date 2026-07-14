# WebAPIDevSecOps

> **This project applies DevSecOps best practices** throughout its lifecycle: static analysis (SAST), dynamic testing (DAST), dependency scanning, SBOM generation, container scanning, automated security testing, and a CI/CD pipeline with security built into every stage.

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-green)

Secure REST API built with ASP.NET Core 10 featuring JWT authentication, Argon2id hashing, rate limiting, and SQL Server, backed by a full DevSecOps CI/CD pipeline.

> For detailed development conventions, test quirks, and quick commands, see [AGENTS.md](AGENTS.md).

---

## Tech Stack

| Category | Technologies |
|---|---|
| **Runtime** | .NET 10, ASP.NET Core 10 |
| **Database** | SQL Server (EF Core) / InMemory (development and tests) |
| **Authentication** | JWT Bearer (HMAC-SHA256) |
| **Hashing** | Argon2id (Konscious) + BCrypt fallback |
| **API Docs** | OpenAPI + Scalar UI |
| **Logging** | Serilog (JSON files + console) |
| **Resilience** | Polly (circuit breaker) |
| **Validation** | FluentValidation |

---

## Security Features

| Feature | Detail |
|---|---|
| **JWT** | 60 min token, HMAC-SHA256, clock skew zero, issuer/audience validation |
| **Password** | Argon2id (64 MB memory, 3 iterations) + BCrypt fallback |
| **Rate Limiting** | Global 1000 req/min, Login 5 req/5min sliding window |
| **Token Blacklist** | Logout invalidates token immediately (in-memory cache) |
| **Security Headers** | CSP, HSTS, X-Frame-Options, X-Content-Type-Options, X-XSS-Protection, Referrer-Policy, Permissions-Policy |
| **Exception Handling** | Middleware mapping exceptions to correct HTTP codes (409, 404, 403, 400, 500) |
| **Account Lockout** | 5 failed login attempts lock the account for 15 minutes |
| **Request Timeout** | 60 seconds, configurable |
| **Kestrel Limits** | Max 1000 concurrent connections, 1 MB body size |
| **CORS** | Single allowed origin (https://localhost:5097) |
| **Anti-enumeration** | Fake hash to prevent timing attacks on login |

---

## Quick Setup

```powershell
# 1. Copy configuration template
cp WebAPIDevSecOps/appsettings.Example.json WebAPIDevSecOps/appsettings.json

# 2. Edit appsettings.json with your local connection
#    Or use in-memory database for quick development:
#    Add "UseInMemoryDatabase": true in appsettings.json

# 3. Restore dependencies and run
dotnet restore
dotnet run --project WebAPIDevSecOps/WebAPIDevSecOps.csproj
```

**Development URLs:**  
- `http://localhost:5196`  
- `https://localhost:7227`  
- API documentation: `/scalar`

---

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/api/v1/login/login` | No | User authentication |
| `POST` | `/api/v1/logout/logout` | No | Invalidate JWT token |
| `GET` | `/api/v1/usuario` | AdminOnly | Paginated user list |
| `GET` | `/api/v1/usuario/{id}` | AdminOnly | Get user by ID |
| `GET` | `/api/v1/usuario/buscar` | AdminOnly | Search users by text |
| `GET` | `/api/v1/usuario/autocomplete` | AdminOnly | User autocomplete |
| `POST` | `/api/v1/usuario` | AdminOnly | Create user |
| `PUT` | `/api/v1/usuario/{id}` | AdminOnly | Update user |
| `DELETE` | `/api/v1/usuario/{id}` | AdminOnly | Delete user |
| `GET` | `/api/v1/cliente` | AdminOnly | Paginated client list |
| `GET` | `/api/v1/cliente/{id}` | AdminOnly | Get client by ID |
| `GET` | `/api/v1/cliente/buscar` | AdminOnly | Search clients by text |
| `GET` | `/api/v1/cliente/autocomplete` | AdminOnly | Client autocomplete |
| `POST` | `/api/v1/cliente` | AdminOnly | Create client |
| `PUT` | `/api/v1/cliente/{id}` | AdminOnly | Update client |
| `DELETE` | `/api/v1/cliente/{id}` | AdminOnly | Delete client |
| `GET` | `/api/v1/tipoempleado` | AdminOnly | Employee type catalog |
| `GET` | `/api/v1/tipoempleado/{id}` | AdminOnly | Employee type by ID |
| `GET` | `/api/v1/empleado` | AdminOnly | Paginated employee list |
| `GET` | `/api/v1/empleado/{id}` | AdminOnly | Get employee by ID |
| `GET` | `/api/v1/empleado/buscar` | AdminOnly | Search employees by text |
| `POST` | `/api/v1/empleado` | AdminOnly | Create employee |
| `PUT` | `/api/v1/empleado/{id}` | AdminOnly | Update employee |
| `DELETE` | `/api/v1/empleado/{id}` | AdminOnly | Delete employee |
| `GET` | `/api/v1/producto` | AdminOnly | Paginated product list |
| `GET` | `/api/v1/producto/{id}` | AdminOnly | Get product by ID |
| `GET` | `/api/v1/producto/buscar` | AdminOnly | Search products by text |
| `POST` | `/api/v1/producto` | AdminOnly | Create product |
| `PUT` | `/api/v1/producto/{id}` | AdminOnly | Update product |
| `DELETE` | `/api/v1/producto/{id}` | AdminOnly | Delete product |
| `GET` | `/api/v1/estadoventa` | AdminOnly | Sale status catalog |
| `GET` | `/api/v1/estadoventa/{id}` | AdminOnly | Sale status by ID |
| `GET` | `/api/v1/venta` | AdminOnly | Paginated sale list |
| `GET` | `/api/v1/venta/{id}` | AdminOnly | Get sale by ID |
| `GET` | `/api/v1/venta/buscar` | AdminOnly | Search sales by folio/client |
| `POST` | `/api/v1/venta` | AdminOnly | Create sale |
| `GET` | `/api/v1/ventadetalle` | AdminOnly | Paginated detail list |
| `GET` | `/api/v1/ventadetalle/{id}` | AdminOnly | Get detail by ID |
| `GET` | `/api/v1/ventadetalle/buscarproducto` | AdminOnly | Product autocomplete for sales |
| `POST` | `/api/v1/ventadetalle` | AdminOnly | Add product to sale |
| `GET` | `/health` | No | Full health checks |
| `GET` | `/health/ready` | No | Database-only health check |

---

## Data Model

| Entity | Table | Description |
|---|---|---|
| `SegUsuario` | `SegUsuario` | Application users (login, roles) |
| `CliCliente` | `CliCliente` | Client catalog |
| `EmpCatTipoEmpleado` | `EmpCatTipoEmpleado` | Employee type catalog |
| `EmpEmpleado` | `EmpEmpleado` | Employee catalog |
| `ProProducto` | `ProProducto` | Product catalog |
| `VenCatEstado` | `VenCatEstado` | Sale status catalog |
| `VenVenta` | `VenVenta` | Sales header |
| `VenVentaDetalle` | `VenVentaDetalle` | Sales detail lines |
| `SegTokenBlacklist` | `SegTokenBlacklist` | *Excluded from migrations — in-memory only* |

---

## Project Structure

```
WebAPIDevSecOps/
├── WebAPIDevSecOps/            # Main API
│   ├── Program.cs               # Entrypoint and middleware pipeline
│   ├── appsettings.json         # Local configuration (not versioned)
│   ├── appsettings.Example.json # Configuration template (versioned)
│   ├── Controllers/             # API endpoints (11 files)
│   ├── Services/                # Business logic
│   ├── Middleware/              # Exception handling, audit logging, timeout
│   ├── Context/                 # EF Core DbContext
│   ├── Models/                  # Database entities (9 entities)
│   ├── Dto/                     # Request/Response models + Validators (33 files)
│   └── Migrations/              # EF Core migrations
├── UnitTest/                    # Unit tests (303 tests)
├── IntegrationTest/             # Integration tests (~160 tests)
└── SecurityTest/                # Security tests (129 tests)
```

---

## CI/CD Pipeline

```
push → restore → build → unit tests → integration tests → security tests
       → vuln check → SBOM → docker build → Trivy scan → DAST (ZAP)
       → SonarCloud → Semgrep (PR only)
```

| Stage | Tool | What it checks |
|---|---|---|
| **Unit Tests** | xUnit + Moq + FluentAssertions | Business logic (303 tests) |
| **Integration Tests** | WebApplicationFactory | Full API against database (~160 tests) |
| **Security Tests** | WebApplicationFactory | SQLi, XSS, JWT, rate limiting, headers (129 tests) |
| **Vulnerable Dependencies** | `dotnet list package --vulnerable` | NuGet packages with known CVEs |
| **SBOM** | CycloneDX | Generates dependency inventory in CycloneDX format |
| **Container Scan** | Trivy | Scans Docker image; HIGH/CRITICAL blocks push |
| **DAST** | OWASP ZAP | Dynamic attacks against the API using OpenAPI spec |
| **SAST** | SonarCloud + Semgrep | Static code analysis with custom rules |

---

## Testing

```powershell
# Unit tests
dotnet test UnitTest/UnitTest.csproj

# Integration tests
dotnet test IntegrationTest/IntegrationTest.csproj

# Security tests
dotnet test SecurityTest/SecurityTest.csproj

# All tests
dotnet test UnitTest/UnitTest.csproj
dotnet test IntegrationTest/IntegrationTest.csproj
dotnet test SecurityTest/SecurityTest.csproj
```

---

## Configuration

Key sections in `appsettings.json`:

| Section | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt` | Key (min 32 bytes), Issuer, Audience |
| `PasswordHashing` | MemorySize, Iterations, DegreeOfParallelism |
| `Resilience` | Circuit breaker parameters |
| `RequestTimeoutSeconds` | Global request timeout |

Database credentials can be overridden via environment variables: `DB_USER` and `DB_PASSWORD`.

---

## Database Migrations

```powershell
# Add a new migration
dotnet ef migrations add MigrationName --project WebAPIDevSecOps --context AppDbContext

# Apply migrations to database
dotnet ef database update --project WebAPIDevSecOps --context AppDbContext

# Remove last migration (if not applied)
dotnet ef migrations remove --project WebAPIDevSecOps --context AppDbContext
```

> **Note:** `SegTokenBlacklist` is configured with `.ExcludeFromMigrations()` — it exists in the model for the in-memory blacklist but no migration will create it in SQL Server.

---

## License

MIT
