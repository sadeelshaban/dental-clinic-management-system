# Dental Clinic API

ASP.NET Core Web API connected to **MariaDB 10.4** (`dental_clinic_db`).

## Database policy

- The SQL script in `../../database/dental_clinic_db.sql` is the **source of truth**.
- EF Core is used in **database-first** mode (scaffolded entities).
- Do **not** run EF migrations against this database.

## Run locally

1. Start XAMPP MariaDB.
2. Ensure `dental_clinic_db` exists (import SQL if needed).
3. Configure your database credentials in `appsettings.json` (see `appsettings.example.json` for reference).
4. Run the API:

```bash
cd backend/DentalClinic.API
dotnet run
```

Swagger UI: `http://localhost:5062`

## Demo login (Development seed)

Demo credentials are seeded during development. Check the `Data/DatabaseSeeder.cs` file for the default demo user credentials.

## API endpoints

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/health` | Public |
| POST | `/api/auth/login` | Public |
| GET | `/api/auth/me` | JWT |
| GET | `/api/patients` | ADMIN, DOCTOR, SECRETARY |
| POST | `/api/patients` | ADMIN, SECRETARY |

## Project structure

```text
DentalClinic.API/
├── Configuration/
├── Constants/
├── Controllers/
├── Data/              # DbContext + seeder
├── DTOs/
├── Models/          # Scaffolded from MariaDB
├── Extensions/
├── Middleware/
└── Services/
```
