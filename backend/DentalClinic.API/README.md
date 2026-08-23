# Dental Clinic API

ASP.NET Core Web API for the Dental Clinic Management System. Connects to **MariaDB 10.4** database **`dental_clinic_db`**.

**Scope:** Backend only. The React frontend is a separate future phase and is not part of this project folder.

Requirements and business rules: [`../../reference.md`](../../reference.md).

## Docker

From the repository root:

```bash
cp .env.example .env
docker compose up -d --build
```

See [`../../docker/README.md`](../../docker/README.md) for details.

## Database setup (database-first)

1. Start MariaDB (e.g. XAMPP).
2. Import the schema from the repository root:

   ```bash
   mysql -u root -p < ../../database/dental_clinic_db.sql
   ```

3. **Do not** run EF Core migrations. The SQL script is the source of truth; models in `Models/` are scaffolded to match it.

## Local run

1. Copy configuration:

   ```bash
   copy appsettings.example.json appsettings.json
   ```

2. Set your MariaDB password and JWT secret in `appsettings.json` (see `appsettings.example.json` for field names). Default JWT expiry in the example is **480 minutes**.

3. Run:

   ```bash
   dotnet restore
   dotnet run
   ```

- **HTTP (default profile):** `http://localhost:5062`
- **HTTPS profile:** `https://localhost:7105` and `http://localhost:5062`
- **Swagger UI (Development):** site root `/`

## Demo login (Development seed)

When `ASPNETCORE_ENVIRONMENT=Development`, the API seeds a demo ADMIN if the database is reachable and the `users` table is empty. See `Data/DatabaseSeeder.cs` for the seeded email; the password is defined there for local use only — **do not use in production**.

## API documentation

**Swagger is the authoritative endpoint catalog** for the current build. It lists every controller, route, request body, and auth requirement.

Manual checks without Swagger: use `DentalClinic.API.http` (health) or any HTTP client.

### Response envelope

All endpoints return `ApiResponse<T>`:

```json
{ "success": true, "message": "optional", "data": { } }
```

### Authentication

- `POST /api/auth/login` — public; rate-limited (5 attempts / 15 minutes / IP)
- Protected routes require `Authorization: Bearer <JWT>`
- JWT claims include `user_id`, `clinic_id`, and role; clinic scope is always taken from the token, never from client input

### Representative routes

| Method | Route | Auth (typical) |
|--------|-------|----------------|
| GET | `/api/health` | Public |
| POST | `/api/auth/login` | Public |
| GET | `/api/auth/me` | JWT |
| POST | `/api/auth/change-password` | JWT |
| GET/POST/PUT/DELETE | `/api/patients` | Clinical staff / ADMIN+SECRETARY writes |
| GET | `/api/patients/{id}/financial` | Clinical staff |
| GET/POST/PUT | `/api/users`, `/api/doctors` | ADMIN (reads: clinical staff for doctors) |
| GET/POST/PUT + status actions | `/api/appointments` | Clinical staff |
| GET/POST/PUT | `/api/visits` | Clinical staff (writes: ADMIN/DOCTOR) |
| GET/POST/PUT | `/api/treatmentcategories`, `/api/treatments` | Reads: clinical staff; writes: ADMIN |
| GET/POST/PUT | `/api/patienttreatments` | Reads: clinical staff; writes: ADMIN/DOCTOR |
| GET/POST | `/api/payments`, POST void | Writes: ADMIN/SECRETARY |
| GET/POST/PUT | `/api/paymentmethods` | Writes: ADMIN |
| GET/POST/PUT/POST void | `/api/expenses`, `/api/expensepayments` | Expense writes: ADMIN/SECRETARY |
| GET/POST/PUT | `/api/suppliers`, `/api/expensecategories` | Per Swagger |
| POST upload, GET download, GET list, DELETE | `/api/attachments` | Upload: ADMIN/SECRETARY; download/list: clinical staff; **files are not served publicly** |
| GET | `/api/reports/daily`, `/monthly`, `/comparison`, `/patient-directory`, `/outstanding-balances` | Financial reports: ADMIN; directory/outstanding: clinical staff |

`/api/reports/daily` returns `{ outstandingPatientBalances, items }` where `items` is the per-day breakdown.

## Project structure

```text
DentalClinic.API/
├── Configuration/       # JwtSettings
├── Constants/           # AppRoles, audit actions/entities
├── Controllers/         # 18 API controllers
├── Data/                # DentalClinicDbContext, DatabaseSeeder
├── DTOs/
├── Models/              # Scaffolded from MariaDB
├── Extensions/          # DI, claims helpers
├── Middleware/          # Exception handling, login rate limit
├── Services/            # Interfaces + implementations
├── Common/              # BusinessRuleException, validators
├── Program.cs
└── appsettings*.json
```

## Production

See `PRODUCTION_DEPLOYMENT.md` and `appsettings.Production.json.example`. Never commit production secrets or `appsettings.Production.json`.
