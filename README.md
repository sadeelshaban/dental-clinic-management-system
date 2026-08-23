# Dental Clinic Management System

Multi-tenant dental clinic management system: ASP.NET Core API, MariaDB, and a React SPA.

**Repository scope:** Backend Phases 0–7 plus Phase 8 frontend (`frontend/`).

Full product requirements and API contracts live in [`reference.md`](reference.md).

## Quick start (backend)

### Option A — Docker (recommended for a consistent environment)

```bash
cp .env.example .env
# Edit .env — set MARIADB_ROOT_PASSWORD and JWT_SECRET (≥ 32 characters)

docker compose up -d --build
```

When containers are **healthy** (`docker compose ps`):

| Link | Purpose |
|------|---------|
| [http://localhost:5173/](http://localhost:5173/) | **Clinic web app** |
| [http://localhost:5062/](http://localhost:5062/) | **Swagger UI** |
| [http://localhost:5062/api/health](http://localhost:5062/api/health) | Health check (JSON) |

**Swagger login:** `POST /api/auth/login` with `admin@demo.com` / `Admin@123`, then **Authorize** with `Bearer <token>`.

See [`docker/README.md`](docker/README.md) for production overrides, volumes, and troubleshooting.

### Option B — Local .NET + MariaDB

#### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MariaDB 10.4+ (e.g. XAMPP on Windows)

### 1. Create the database

The schema is **database-first**. Import the SQL script (do **not** use EF migrations):

```bash
# From the repository root — adjust user/host as needed
mysql -u root -p < database/dental_clinic_db.sql
```

This creates `dental_clinic_db` with tables, views, and demo clinic data.

### 2. Configure the API

```bash
cd backend/DentalClinic.API
copy appsettings.example.json appsettings.json   # Windows
# cp appsettings.example.json appsettings.json   # Linux/macOS
```

Edit `appsettings.json`:

- Set `ConnectionStrings:DentalClinicDb` (database name **`dental_clinic_db`**, port **3306**).
- Set `Jwt:Secret` to a random string of **at least 32 characters**.
- Optional: `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes` (default **480** in the example file).

See `appsettings.example.json` for the expected shape. **Do not commit real secrets.**

### 3. Run the API

```bash
dotnet restore
dotnet run
```

Default dev URL: **`http://localhost:5062`** (see `Properties/launchSettings.json`).

- **Swagger UI:** `http://localhost:5062/` (Development only)
- **Health check:** `GET /api/health` (public)

In Development, if the database is reachable and no users exist, a demo ADMIN is seeded automatically. Credentials are defined in `Data/DatabaseSeeder.cs` (not repeated here).

### 4. Explore the API

Use Swagger for the **complete, current endpoint list**. All responses use the envelope:

```json
{ "success": true, "message": "...", "data": { } }
```

Authenticate via `POST /api/auth/login`, then use **Authorize** in Swagger with `Bearer <token>`.

More detail: [`backend/DentalClinic.API/README.md`](backend/DentalClinic.API/README.md).

## API modules (summary)

| Area | Base routes | Notes |
|------|-------------|--------|
| Health | `/api/health` | Public liveness + DB check |
| Auth | `/api/auth/*` | Login, `/me`, change password; login rate-limited |
| Patients | `/api/patients` | CRUD + `/financial` statement |
| Users / Doctors | `/api/users`, `/api/doctors` | ADMIN-managed users; doctor profiles |
| Appointments | `/api/appointments` | Scheduling, overlap prevention, status lifecycle |
| Clinical | `/api/visits`, `/api/treatmentcategories`, `/api/treatments`, `/api/patienttreatments` | Visits, catalog, patient treatments |
| Billing | `/api/payments`, `/api/paymentmethods` | Partial payments, voids, server-derived status |
| Expenses | `/api/expensecategories`, `/api/suppliers`, `/api/expenses`, `/api/expensepayments` | Obligations, supplier statements, voids |
| Attachments | `/api/attachments` | Upload (ADMIN/SECRETARY); download requires JWT |
| Reports | `/api/reports/*` | Financial summaries, directory, outstanding balances (ADMIN / role rules apply) |

There is **no** `/api/invoices`, `/api/clinics`, or public `/uploads/` static file serving.

## Database policy

- **Source of truth:** `database/dental_clinic_db.sql`
- **EF Core:** database-first (scaffolded models); **never run `dotnet ef migrations`**
- Schema changes require an approved SQL update to the script and an entry in `reference.md` §6

## Production configuration

Use environment variables or `appsettings.Production.json` (gitignored). Template: `backend/DentalClinic.API/appsettings.Production.json.example`.

| Variable | Purpose |
|----------|---------|
| `ConnectionStrings__DentalClinicDb` | MariaDB connection string |
| `Jwt__Secret` | Signing key (≥ 32 characters) |
| `Jwt__ExpirationMinutes` | Token lifetime (optional) |
| `Jwt__Issuer` / `Jwt__Audience` | Optional overrides |

See [`backend/DentalClinic.API/PRODUCTION_DEPLOYMENT.md`](backend/DentalClinic.API/PRODUCTION_DEPLOYMENT.md) for deployment notes.

## Build

```bash
dotnet build backend/DentalClinic.API/DentalClinic.API.csproj
```

## Frontend

React + Vite + TypeScript in `frontend/`.

```bash
cd frontend
copy .env.example .env
npm install
npm run dev
```

Dev server: **http://localhost:5173**. Set `VITE_API_BASE_URL` (default `http://localhost:5062`).

Attachments download only via authenticated `GET /api/attachments/{id}/download`.

## License

[Add your license information here]
