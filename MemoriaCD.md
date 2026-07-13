# Memoria de Continuous Deployment (CD)

## Pipeline CI/CD completo

Archivo: `.github/workflows/ci-cd.yml`

Jobs ejecutados en cada push a `main`:

| Job | Descripción | Duración aprox |
|---|---|---|
| `build-and-test` | Restore → Build → Unit/Integration/Security Tests → Vulnerabilidades → SBOM | ~3 min |
| `docker-build` | Build Docker → Push a Docker Hub → Cosign → Trivy | ~3 min |
| `sonarcloud` | Análisis SAST + cobertura + Quality Gate | ~3 min |
| `fuzz` | RESTler fuzzing sobre API | ~15 min |
| `dast` | OWASP ZAP API scan | ~15 min |
| `deploy-digitalocean` | **Deploy a DO App Platform** + health check + smoke test + rollback | ~4 min |

## Job `deploy-digitalocean` (líneas 486-545)

```yaml
deploy-digitalocean:
  needs: docker-build
  if: github.ref == 'refs/heads/main'
```

### Pasos del CD

1. **Install doctl** — autentica con `DO_API_TOKEN` (GitHub Secret)
2. **Save previous deployment ID** — captura el deployment activo actual para rollback
3. **Create deployment** — ejecuta `doctl apps create-deployment --wait` que:
   - Triggera un nuevo deploy en DO App Platform
   - DO descarga la nueva imagen desde Docker Hub
   - DO despliega el contenedor con las env vars configuradas
   - `--wait` espera a que el deployment termine (active/error)
4. **Verify health** — curl a `/health/ready` cada 10s (hasta 12 intentos = 2 min)
5. **Smoke test** — `bash scripts/smoke-test.sh` (login opcional)
6. **Rollback** — si falla, revierte al deployment anterior

## Integración con DigitalOcean

- **MCP configurado** en `opencode.json` (6 servicios DO)
- **DO App Platform** creada con:
  - URL: `https://webapidevopsproject-h5fn4.ondigitalocean.app`
  - Health check: `/health/ready` con 120s delay
  - Autodeploy: OFF (controlado por GitHub Actions)
  - Env vars: `ConnectionStrings__DefaultConnection`, `Jwt__*`, `ASPNETCORE_ENVIRONMENT`
- **Persistencia**: TokenBlacklist en tabla `SegTokenBlacklist` via EF migraciones

## Mejoras de seguridad deployadas

- **ForwardedHeaders** — DO LB termina SSL, headers `X-Forwarded-Proto` manejados correctamente
- **CORS** — fix para `AllowedOrigin` vacío en producción
- **Health check "self"** — visibilidad siempre presente

## Resultados finales

| Ítem | Detalle |
|---|---|
| App URL | `https://webapidevopsproject-h5fn4.ondigitalocean.app` |
| SQL Server | `db45497.public.databaseasp.net` |
| BD | Persistente |
| CI/CD | Automático en cada push a `main` |
| Rollback | Automático si health check o smoke test fallan |

## GitHub Secrets y Variables requeridos

### Secrets
- `DO_API_TOKEN` — Token de DigitalOcean
- `DO_APP_ID` — ID de la app en DO
- `DOCKER_USERNAME` — Usuario de Docker Hub
- `DOCKER_PASSWORD` — Access Token de Docker Hub
- `ConnectionStrings__DefaultConnection` — Cadena de conexión SQL Server
- `Jwt__Key` — Clave 256-bit para JWT
- `DB_USER` — Usuario SQL (opcional si ya va en connection string)
- `DB_PASSWORD` — Contraseña SQL (opcional si ya va en connection string)
- `SONAR_TOKEN` — Token de SonarCloud

### Variables
- `DO_APP_URL` — `https://webapidevopsproject-h5fn4.ondigitalocean.app`
- `SONAR_PROJECT_KEY` — Proyecto en SonarCloud
- `SONAR_ORG` — Organización en SonarCloud
