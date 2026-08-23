# Docker

Run the Dental Clinic API and MariaDB with Docker Compose.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine + Compose v2)

## Quick start (development)

From the repository root:

```bash
cp .env.example .env
# Edit .env — set MARIADB_ROOT_PASSWORD and JWT_SECRET (≥ 32 characters)

docker compose up -d --build
```

Wait until both services are healthy (`docker compose ps` — both **healthy**), then open these URLs in your browser:

| What | URL | Status |
|------|-----|--------|
| **Swagger UI** (test all API endpoints) | [http://localhost:5062/](http://localhost:5062/) | Opens API docs at `/index.html` |
| **OpenAPI JSON** | [http://localhost:5062/swagger/v1/swagger.json](http://localhost:5062/swagger/v1/swagger.json) | Raw spec |
| **Health check** | [http://localhost:5062/api/health](http://localhost:5062/api/health) | JSON liveness + DB check |

Default host port **5062** matches local `dotnet run` (see `API_PORT` in `.env`).

### Swagger login (Development)

1. Open [http://localhost:5062/](http://localhost:5062/)
2. Expand **POST /api/auth/login** → **Try it out**
3. Use demo credentials (seeded on first run when no users exist):

| Field | Value |
|-------|--------|
| email | `admin@demo.com` |
| password | `Admin@123` |

4. Copy `data.token` from the response
5. Click **Authorize** (top right) → enter: `Bearer <paste-token>` → **Authorize**

All protected endpoints are now callable from Swagger — no separate `dotnet run` needed.

## Commands

```bash
# View logs
docker compose logs -f api

# Rebuild after code changes
docker compose up -d --build api

# Stop and remove containers (keeps volumes)
docker compose down

# Stop and remove containers + database/uploads volumes (full reset)
docker compose down -v
```

## Production

1. Copy `.env.example` to `.env` and use strong secrets.
2. Start with production overrides (no Swagger/seeder, MariaDB not published to host):

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

3. Put a reverse proxy (nginx, Caddy, Traefik) in front of the API for TLS.

## Architecture

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐
│   Browser   │────▶│  api :8080   │────▶│ mariadb :3306   │
│ localhost   │     │  (ASP.NET)   │     │ MariaDB 10.4    │
│ :5062       │     └──────────────┘     └─────────────────┘
└─────────────┘            │
                           ▼
                    volume: api_uploads
```

## Environment variables

Set in `.env` (see `.env.example`):

| Variable | Purpose |
|----------|---------|
| `MARIADB_ROOT_PASSWORD` | MariaDB root password (required) |
| `MARIADB_PORT` | Host port for MariaDB (default 3306) |
| `API_PORT` | Host port for API (default 5062) |
| `JWT_SECRET` | JWT signing key, ≥ 32 chars (required) |

The API container also receives standard ASP.NET configuration keys (`ConnectionStrings__DentalClinicDb`, `Jwt__*`, `Uploads__Path`) via `docker-compose.yml`.

## Files

| File | Purpose |
|------|---------|
| `docker-compose.yml` | Dev stack: MariaDB + API |
| `docker-compose.prod.yml` | Production overrides |
| `backend/DentalClinic.API/Dockerfile` | Multi-stage .NET 10 build |
| `backend/DentalClinic.API/.dockerignore` | Exclude bin/obj and local secrets from image |
| `.env.example` | Template for secrets (copy to `.env`) |

## Notes

- **Database-first:** schema comes from `database/dental_clinic_db.sql` on first volume init only. To re-import, run `docker compose down -v` then `up` again (destroys data).
- **Uploads** persist in the `api_uploads` Docker volume.
- **No EF migrations** — same policy as local development.
