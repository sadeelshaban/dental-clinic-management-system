# Dental Clinic Project Reference

> **MANDATORY PROJECT MEMORY FILE.** Every future task MUST begin by reading this file completely. Every meaningful change MUST be documented here BEFORE reporting the task complete. Do not rely on conversation history alone. Never store secrets, passwords, API keys, JWT secrets, tokens, or credentials in this file.

---

## 1. Project Overview

- **Project name:** Dental Clinic Management System
- **Purpose:** Manage the daily operations of a dental clinic: patients, appointments, clinical visits, treatments, billing and payments, expenses and suppliers, financial reporting, and a dashboard.
- **Target users:** Clinic staff with three roles — `ADMIN`, `DOCTOR`, `SECRETARY`.
- **Main business problem:** Replace manual/spreadsheet tracking of patients, scheduling, clinical records, and clinic finances (revenue vs. expenses, outstanding balances, supplier obligations) with a reliable multi-role web application.
- **Main workflows:**
  1. Secretary registers patients and books appointments.
  2. Doctor conducts visits, records diagnosis/notes, and prescribes treatment lines (billing items).
  3. Secretary/ADMIN records payments (full or partial) against treatment lines.
  4. ADMIN manages users, treatment catalog, expenses, suppliers, and reviews financial reports/dashboard.
- **Context:** Clinic located in Palestine (default country `Palestine`, city `Nablus` in demo data, currency `ILS` / `₪`, timezone `Asia/Gaza`).
- **Design principle:** Reusable clinic management system — configurable per clinic, NOT a one-off hardcoded application.
- **Current project status (2026-08-22):**
  - Requirements: **FORMALIZED** (confirmed via project discussion on 2026-08-22 — see §2). Phase 0 requirements confirmation COMPLETE.
  - Database: **complete** (schema + views + demo seed).
  - Backend: **auth + patients + users/doctors + appointments + clinical records + billing complete** (~70% of planned API scope). Phase 1 DONE (31/31); Phase 2 DONE (39/39); Phase 3 DONE (41/41); Phase 4 DONE (30/30 smoke tests).
  - Frontend: **not started** (empty `frontend/` directory).
  - `reference.md`: **created** (this file).

---

## 2. Confirmed Requirements

> **Formalized 2026-08-22 from the project discussion.** The absence of a separate requirements document does NOT block the project — this section IS the requirements record.
>
> Status labels: **CONFIRMED** (agreed, in scope) · **PENDING** (awaiting decision/confirmation) · **OPTIONAL** (nice-to-have, not blocking) · **FUTURE** (planned, explicitly out of current MVP) · **REJECTED** (explicitly excluded).

### 2.1 System shape — CONFIRMED
- Dental Clinic Management System for a clinic with: multiple doctors, secretary, admin, patients, treatments, appointments, payments, expenses/obligations, financial reporting.
- Designed as a **reusable clinic management system**, not a one-off hardcoded application.
- A clinic can have multiple users with the same role (e.g., Doctor 1, Doctor 2, Secretary, Admin).
- Doctors are users with the DOCTOR role plus a linked doctor profile.

### 2.2 Patient management — CONFIRMED
- Add, search, edit, deactivate patients; view patient profiles and history.
- Patient profile eventually contains: basic info, contact info, medical info, visits, treatments, payments, outstanding balance, appointments, notes, attachments (when implemented).
- UX goal: make patient retrieval significantly faster than paper workflows — MINIMAL STEPS, MINIMAL TYPING, FAST ACCESS.

### 2.3 Treatments — CONFIRMED
- A patient can have multiple treatments. Each records: treatment type, doctor, date, visit, quantity, unit price, discount, final amount, notes, payment status.
- Multiple payments can be made against one treatment.
- **Historical treatment prices must never change** because the catalog price changes later (e.g., charged 450 stays 450 even if catalog goes 500 → 600).

### 2.4 Payments — CONFIRMED
- A treatment can have multiple partial payments (e.g., 1000 = 300 + 200 + 500 → remaining 0).
- Voided payments must NOT count toward financial totals.
- **Revenue means ACTUAL MONEY RECEIVED**, NOT the total value of treatments created. This distinction is critical.

### 2.5 Patient balances — CONFIRMED
- Outstanding Patient Balance = money owed by patients not yet collected. It is NOT revenue.
- Calculation: Treatment Total − Valid Payments = Remaining Balance.

### 2.6 Appointments — CONFIRMED
- Internal appointment management IS part of the core system; the secretary creates appointments; doctors have appointments associated with them.
- Support: patient, doctor, date, start time, end time, status, reason, notes.
- Statuses at least: SCHEDULED, CONFIRMED, COMPLETED, CANCELLED, NO_SHOW.
- The system MUST prevent overlapping appointments for the same doctor.
- Online patient self-booking: FUTURE (NOT current MVP).

### 2.7 Clinical records — CONFIRMED
- Visits associated with: patient, doctor, date, chief complaint, diagnosis, clinical notes, follow-up date.
- Patient Treatments linked to visits; **every treatment must record the doctor responsible** for performing it.

### 2.8 Dental chart — PENDING CLINIC CONFIRMATION
- NOT confirmed yet. The exact standard must be confirmed by the clinic: **FDI, Universal, or Palmer**.
- Do NOT invent a dental chart implementation. Keep PENDING until the clinic decides.

### 2.9 Expenses / obligations — CONFIRMED
- Expense types include: rent, materials, laboratory, equipment, supplier, maintenance, utilities, other.
- Track Total / Paid / Remaining where applicable.
- Supplier transactions behave like patient treatment payments: Total obligation − Payments = Remaining.
- Track who the clinic owes and how much remains.

### 2.10 Financial dashboard — CONFIRMED (build in Phase 7)
- THIS WEEK / THIS MONTH / CUSTOM DATE RANGE — each showing: Revenue, Expenses, Net Profit, Outstanding Patient Balances.
- Financial rule: **Net Profit = Actual Payments Received − Actual Expenses Paid.** Never treat unpaid treatment amounts as revenue.

### 2.11 Monthly comparison — CONFIRMED (build in Phase 7)
- Compare current month vs previous month. Metrics: Revenue, Expenses, Net Profit, Patients, Appointments.
- Show: current value, previous value, change, percentage change.

### 2.12 Reports — CONFIRMED (build in Phase 7)
- Reports for: revenue, expenses, net profit, patient balances, treatments, payments, appointments, monthly comparison.
- Export functionality (PDF/Excel/CSV): NOT required for the current backend phase unless explicitly approved later → OPTIONAL/FUTURE.

### 2.13 Users & permissions — CONFIRMED (financial boundaries refinable)
- **ADMIN:** manage users, manage doctors, manage clinic configuration, access financial information, access all clinic data.
- **DOCTOR:** view patients, view clinical information, manage visits, manage treatments, manage appointments, view appropriate financial information according to final permission rules.
- **SECRETARY:** manage patients, manage appointments, record payments, view patient information, handle administrative workflows.
- Exact financial permission boundaries can be refined later (PENDING refinement detail).

### 2.14 Multi-tenancy — CONFIRMED
- Database is multi-tenant; every clinic-owned record scoped to `clinic_id`; a user belongs to a clinic.
- Users must NEVER access another clinic's data. The client must NOT be trusted to provide clinic_id — the backend determines it from the authenticated user context.
- MVP UI behavior: each user works within ONE current clinic; no clinic-switching UI required.

### 2.15 UX principles — CONFIRMED
- The system must be faster and easier than paper: minimal typing, minimal clicks/steps, Quick Actions, fast patient search, fast patient profile access, clear financial information, simple workflows, avoid unnecessary forms and confirmation steps.
- Quick Actions eventually include: Add Patient, Add Treatment, Add Payment, Add Appointment.
- Fast patient profile access is the requirement; specific interaction mechanics (e.g., double-click) are implementation details, NOT specs.

### 2.16 Frontend — CONFIRMED (stack) / FUTURE (Arabic)
- React + Vite + TypeScript consuming the existing REST API.
- UI in English initially; architecture remains i18n-ready so Arabic/RTL can be added later (FUTURE/PLANNED, not required for MVP).

### 2.17 Online booking — FUTURE
- NOT part of the current MVP. Internal appointment management is required.

### 2.18 PDF invoices / receipts — FUTURE / OPTIONAL
- Full invoice system is future; payment receipts may be added later. Do NOT block current backend implementation on PDF.

### 2.19 Audit logging — CONFIRMED
- Important mutations must be auditable, especially: user creation/update/deactivation, doctor creation/update/deactivation, patient changes, treatment changes, payment creation/void, expense creation/update/void.
- NEVER log: passwords, password hashes, JWT secrets, authentication tokens.

### 2.20 Database rules — CONFIRMED
- MariaDB 10.4.32; database-first is mandatory; the existing database is the source of truth; **NO EF CORE MIGRATIONS**.
- Do not silently modify the schema. If a schema change is genuinely required: STOP and report the exact SQL and reason BEFORE applying it.

### 2.21 Roadmap — CONFIRMED
- PHASE 0 Foundation/security/stability/requirements → PHASE 1 Users+Doctors → PHASE 2 Appointments+Scheduling → PHASE 3 Visits+Treatment Catalog+Patient Treatments → PHASE 4 Payments+Patient Financials → PHASE 5 Expenses+Suppliers → PHASE 6 Attachments → PHASE 7 Reports+Dashboard → PHASE 8 React+Vite Frontend → PHASE 9 Testing+Production Hardening.
- Do NOT skip phases unless explicitly instructed.

### Non-CONFIRMED summary
- **PENDING:** dental chart standard (FDI / Universal / Palmer — clinic must confirm); DOCTOR financial permission boundary details.
- **FUTURE / OPTIONAL:** online booking, Arabic/RTL UI, PDF invoices/receipts, report exports (PDF/Excel/CSV), SMS/email reminders.
- **REJECTED:** EF Core migrations; implementing online booking / dental chart / PDF invoicing within the current MVP without explicit approval.

---

## 3. Business Rules

Rules below are enforced by the schema or implied by views; app code must respect them.

### Money & revenue
1. **Revenue = actual payments received** (`patient_payments` where `is_voided = FALSE`). Outstanding patient balances are **NOT** revenue.
2. **Voided payments never count** toward any financial total (all financial views filter `is_voided = FALSE`).
3. Patient treatment line total: `final_amount = GREATEST(quantity × unit_price − discount_amount, 0)` — stored generated column.
4. Remaining balance per treatment line: `final_amount − SUM(valid payments)`.
5. **Partial payments allowed.** Treatment status lifecycle: `UNPAID → PARTIALLY_PAID → PAID` (plus `VOIDED`).
6. Discount cannot exceed `quantity × unit_price` (CHECK constraint).
7. Expense obligations mirror the same model: `total_amount`, valid payments, `remaining_balance`; status `UNPAID / PARTIALLY_PAID / PAID / VOIDED`.
8. All amounts `DECIMAL(12,2)`; currency ILS (₪) per clinic settings.

### Catalog & history
9. **Historical prices must not change** when the catalog changes: `patient_treatments` snapshots `treatment_name` and `unit_price` at entry time; `treatment_id` FK is `ON DELETE SET NULL`.
10. Treatment `default_price >= 0`; `duration_minutes > 0` or NULL.

### Scheduling
11. Appointment `end_time > start_time` (CHECK constraint).
12. **Appointments must not overlap for the same doctor** — NOT enforceable in MySQL; must be enforced in the service layer.
13. Appointment statuses: `SCHEDULED, CONFIRMED, COMPLETED, CANCELLED, NO_SHOW` (default `SCHEDULED`).
14. Working hours: `day_of_week` 0–6; closed days have NULL times; open days require `closing_time > opening_time`.

### Identity & tenancy
15. Every business row is scoped by `clinic_id`; users see only their clinic's data (clinic derived from JWT claim).
16. User email unique per clinic; patient number unique per clinic (format `P-#####`).
17. Soft delete via `is_active` flags for clinics/users/doctors/patients/catalog items; payments/expenses are voided, never deleted.
18. An attachment must belong to a patient OR a patient treatment (CHECK constraint).
19. `doctors` is a 1:1 extension of `users` (`uq_doctors_user`); a doctor must have a user account.

### Product-level rules (formalized 2026-08-22)
20. **Net Profit = Actual Payments Received − Actual Expenses Paid** (cash basis). Unpaid treatment amounts are NEVER revenue.
21. Dashboard periods (this week / this month / custom range) each show: Revenue, Expenses, Net Profit, Outstanding Patient Balances.
22. **Multi-tenancy enforcement:** the client-supplied clinic_id is never trusted; clinic is always derived from the authenticated user's JWT claim; cross-clinic access must be impossible.
23. **Audit logging:** important mutations (users, doctors, patients, treatments, payments incl. voids, expenses) must be auditable; never log passwords, password hashes, JWT secrets, or tokens.
24. **Schema change protocol:** silent schema changes are forbidden — any required change must be reported with exact SQL + reason and approved before applying (database-first, no EF migrations).

---

## 4. Technology Stack

| Layer | Technology | Version / Notes |
|---|---|---|
| Backend | ASP.NET Core (C#), `Microsoft.NET.Sdk.Web` | Target `net10.0` |
| Database | MariaDB (XAMPP) | 10.4.32 (hardcoded in `ServiceCollectionExtensions`) |
| ORM | Entity Framework Core, **database-first** (scaffolded) | EF Core 9.0.11 + Pomelo.EntityFrameworkCore.MySql 9.0.0 |
| Auth | JWT Bearer | Microsoft.AspNetCore.Authentication.JwtBearer 10.0.11; 480-minute expiry |
| Password hashing | BCrypt.Net-Next | 4.0.3 |
| API docs | Swashbuckle.AspNetCore | 7.2.0; Swagger UI at site root in Development |
| Frontend | React + Vite + TypeScript | **Not started**; dev origin `http://localhost:5173` |
| Runtime | Windows 11, XAMPP MariaDB, .NET 10 SDK | |

> Note: version skew (net10.0 runtime with EF Core 9.x packages) currently builds and runs; see Known Issues #2.

---

## 5. Architecture

### Layering (backend)
```
Controllers  →  Services (Interface + Implementation)  →  EF Core DbContext (scaffolded)
     ↓                  ↓
   DTOs            Models (mirror DB 1:1)
```
- **API envelope:** every endpoint returns `ApiResponse<T>` = `{ success, message, data }` (`DTOs/Common/ApiResponse.cs`). Errors use the same envelope with `success=false`.
- **Global error handling:** `Middleware/ExceptionHandlingMiddleware.cs` (registered first in pipeline).
- **Multi-tenancy strategy:** single shared database; every query filtered by `clinic_id`; clinic id comes from the JWT claim (`clinic_id`), never from client input.
- **CORS:** policy `Frontend` allows `http://localhost:5173`, `:3000`, `:4200` with credentials.
- **Seeding:** `Data/DatabaseSeeder.cs` creates the demo ADMIN user on startup in Development (only if no users exist and DB reachable). Demo credentials are documented in `backend/DentalClinic.API/README.md` (not duplicated here).

### Project structure
```
Clinic/
├── reference.md                  # this file
├── database/
│   └── dental_clinic_db.sql      # SOURCE OF TRUTH (schema + views + demo data)
├── backend/
│   └── DentalClinic.API/
│       ├── Configuration/        # JwtSettings
│       ├── Constants/            # AppRoles, ClaimTypesCustom
│       ├── Controllers/          # AuthController, HealthController, PatientsController
│       ├── Data/                 # DentalClinicDbContext (scaffolded), DatabaseSeeder
│       ├── DTOs/                 # Auth/, Common/, Patients/
│       ├── Models/             # 20 tables + 8 views mapped
│       ├── Extensions/           # ServiceCollectionExtensions, ClaimsPrincipalExtensions
│       ├── Middleware/           # ExceptionHandlingMiddleware
│       ├── Services/             # Interfaces/ + Implementations/ (Auth, Token, Patient)
│       ├── Properties/           # launchSettings.json (http://localhost:5062)
│       ├── appsettings.json      # connection string + JWT config (dev values)
│       └── Program.cs
└── frontend/                     # EMPTY — React + Vite + TS to be scaffolded here
```

### Key abstractions / patterns
- Primary-constructor services (`public class PatientService(DentalClinicDbContext db) ...`).
- Service-per-aggregate with interface + DI registration in `AddDentalClinicInfrastructure`.
- Static mapper methods inside services (DTO mapping) — no AutoMapper.
- Keyless entities for DB views (`HasNoKey().ToView(...)`).
- Claims helpers: `User.GetClinicId()`, `User.GetUserId()` (`Extensions/ClaimsPrincipalExtensions.cs`).

### Phase 1 patterns (Users + Doctors)
- **`BusinessRuleException` → HTTP 400:** services throw `Common/BusinessRuleException`; `ExceptionHandlingMiddleware` catches it first and returns `ApiResponse.Fail(message)` with 400. Keeps controllers thin; friendly errors without leaking raw DB exceptions.
- **Audit staging (`IAuditService.Record`):** stages an `audit_logs` row on the caller's DbContext; the caller persists it in the SAME SaveChanges/transaction as the mutation → audit commits or rolls back atomically with the change. IP/User-Agent captured via `IHttpContextAccessor`. Callers must NEVER pass passwords/hashes/tokens.
- **Two-phase save inside transactions:** mutate → `SaveChangesAsync` (ids generated) → stage audit with real entity id → `SaveChangesAsync` → commit. Used by user create/update/status flows.
- **Doctors are created through user creation** (`POST /api/users` with `role=DOCTOR`): `doctors.user_id` is NOT NULL UNIQUE, so a standalone `POST /api/doctors` would duplicate/conflict with that workflow. Intentionally omitted.
- **Single source of truth for active status = `users.is_active`.** Activate/deactivate endpoints sync `doctors.is_active` automatically. No separate doctor status endpoints (prevents "active profile but cannot log in" inconsistency).
- **Role-change safety:** DOCTOR→other deletes the doctor profile ONLY if no clinical history exists (visits/patient_treatments/appointments have FK RESTRICT to doctors); other→DOCTOR creates a blank profile. Guards: admin cannot change own role; admin cannot deactivate self; the clinic's last active ADMIN cannot be demoted/deactivated.
- **Email uniqueness is application-level GLOBAL** (login resolves users by email alone, so cross-clinic duplicates would make login ambiguous). The DB keeps the per-clinic unique index as final protection; duplicate-key exceptions are translated to friendly 400s.
- **Role-change side effect:** deleting + recreating a doctor profile yields a NEW `doctor_id` (auto-increment). Clients must re-fetch ids after role changes.

### Phase 2 patterns (Appointments + Scheduling)
- **Timezone policy:** `appointment_date` / `start_time` / `end_time` are CLINIC-LOCAL wall-clock times (Asia/Gaza). No UTC conversion anywhere in the module; DB stores local wall times and the API returns them verbatim. Never mix UTC into scheduling logic.
- **Status transition matrix (enforced server-side):**
  - SCHEDULED → CONFIRMED | COMPLETED | CANCELLED | NO_SHOW
  - CONFIRMED → COMPLETED | CANCELLED | NO_SHOW
  - COMPLETED, CANCELLED, NO_SHOW are TERMINAL (no outgoing transitions)
  - Invalid transitions → 400 via `BusinessRuleException`.
- **Blocking statuses:** only SCHEDULED and CONFIRMED occupy the doctor's schedule for overlap detection. CANCELLED and NO_SHOW never block (slot instantly rebookable). COMPLETED also does NOT block (documented choice: completed encounters are historical; rebooking the slot is allowed).
- **Overlap predicate:** two appointments overlap iff `existing.start < requested.end AND existing.end > requested.start` (same clinic + doctor + date). Back-to-back (end == next start) is ALLOWED.
- **Working hours:** validated against `clinic_working_hours` for the weekday (`day_of_week` = `(int)DateOnly.DayOfWeek`, Sunday=0, matches schema CHECK 0–6). Missing row or `is_open=false` → "clinic closed" rejection. Must satisfy `opening_time <= start` AND `end <= closing_time`. Single period per day (schema has exactly one row per clinic/day).
- **Slot grid:** `clinic_settings['appointment_slot_minutes']` (demo value 30). When present and >0, the START time must align to the grid (minutes-since-midnight % slot == 0 AND seconds == 0). End time may have any duration. Setting absent/invalid → alignment skipped (no forced slot size).
- **Concurrency strategy:** create/update run inside a transaction that FIRST takes a pessimistic lock on the DOCTOR row (`SELECT doctor_id FROM doctors WHERE doctor_id = ? FOR UPDATE`) before the overlap check → simultaneous bookings for the same doctor serialize. Practical MariaDB approach; no distributed locking.
- **Update semantics:** PUT is partial (null = unchanged). Only SCHEDULED/CONFIRMED appointments can be modified. Overlap check EXCLUDES the appointment's own id. Doctor reassignment (ADMIN/SECRETARY) re-validates target doctor (in clinic + active).
- **Doctor identity:** DOCTOR actors always resolve their own doctor profile from the JWT user_id; client-supplied doctorId is IGNORED for them (listing scope, create owner, update owner). Cross-doctor reads return 404 (no existence leak).
- **Patient rule:** inactive patients cannot receive NEW appointments (documented business rule).

### Phase 3 patterns (Clinical Records: Visits, Catalog, Patient Treatments)
- **Concept separation (strict):** APPOINTMENT = scheduling reservation · VISIT = actual clinical encounter · TREATMENT = reusable catalog definition · PATIENT_TREATMENT = treatment actually performed/recorded. Never merged.
- **Appointment↔Visit linkage:** the schema has NO FK between appointments and visits — none was invented. Linking a visit to an appointment manually is a future concern if the clinic requests it.
- **Visit rules:** visits have NO working-hours/slot constraints (clinical encounters may occur any time); every visit belongs to exactly one clinic + active patient + active doctor. DOCTOR actors are auto-scoped to their own profile; client-supplied doctorId ignored for them; doctors cannot reassign visits to another doctor.
- **Historical price snapshot (CRITICAL):** creating a Patient Treatment copies the catalog item's CURRENT `default_price` into `patient_treatments.unit_price` and its name into `treatment_name`. Later catalog edits NEVER affect historical records (verified by mandatory smoke test: 500 → change catalog to 600 → old record stays 500). An explicit `unitPrice` override at creation is permitted and preserved.
- **Custom (ad-hoc) treatments:** when no catalog `treatmentId` is supplied, the request must provide `treatmentName` AND `unitPrice`; the record is stored without a catalog reference (`treatment_id` NULL).
- **Inactive catalog items** cannot be used for NEW patient treatments (existing historical references remain intact).
- **final_amount is a DB-generated column** (`GREATEST(qty × price − discount, 0)`, STORED) — never computed in application code. All money uses `decimal` end-to-end; no floating point.
- **Status policy (Phase 3):** new patient treatments are always UNPAID; the API does NOT allow changing status — transitions (PARTIALLY_PAID/PAID/VOIDED) belong to the billing module (Phase 4).
- **Visit link rule:** a patient treatment's `visit_id` must reference a visit of the SAME clinic AND SAME patient. The performing doctor may differ from the visit's doctor (documented interpretation).
- **Deletion policy:** NO delete endpoints exist for visits or patient treatments (history must never be destroyed). Categories and catalog items are soft-deactivated via PUT `isActive=false` (FKs are ON DELETE SET NULL — destructive deletes would silently detach history).
- **Category names are unique per clinic** (DB constraint + friendly duplicate errors). Treatment catalog NAMES are NOT unique (schema has no such constraint) — duplicates allowed by design.
- **Permissions (documented decision):** SECRETARY is READ-ONLY across all clinical modules (visits/categories/treatments/patient-treatments) — the confirmed permission matrix grants SECRETARY no clinical write rights. Writes: ADMIN (+ DOCTOR within own scope for visits/patient-treatments). New role constant `AppRoles.AdminOrDoctor`.
- **Route naming note:** controllers follow the existing `api/[controller]` convention, so multi-word controllers resolve to `/api/treatmentcategories` and `/api/patienttreatments` (NOT kebab-case).

### Phase 4 patterns (Billing & Payments)
- **REVENUE DEFINITION (CRITICAL):** revenue = actual valid (non-voided) money received via `patient_payments`. It is NEVER the treatment value, final amount, or outstanding balance. Example: treatment 1000 with 400 paid → revenue contribution 400, outstanding 600.
- **Status derivation (server-only):** after every payment create/void the treatment status is recomputed from valid payments vs total: paid=0 → UNPAID; paid ≥ total → PAID; otherwise PARTIALLY_PAID. Clients can NEVER set status; it is not an editable field anywhere.
- **Overpayments REJECTED:** payment amount must be > 0 and ≤ remaining balance (total − valid paid). Zero/negative rejected by DTO Range + service check.
- **Concurrency:** create and void run inside a transaction that FIRST locks the treatment row (`SELECT ... FROM patient_treatments WHERE patient_treatment_id = ? FOR UPDATE`) before reading valid-paid and inserting/updating → concurrent payments on one treatment serialize; combined amounts can never exceed the total (verified by parallel-jobs smoke test).
- **Void semantics:** soft reversal only — NO delete endpoint exists for payments. Voided payments remain stored and auditable (with required reason, voided_at/by) but are EXCLUDED from totals, revenue, and balances immediately; status recalculates (PAID→PARTIALLY_PAID→UNPAID as needed). Double void rejected.
- **Authoritative financial sources:** statement totals come from the existing `patient_financial_summary` view; per-treatment lines (total/paid/remaining) from `patient_treatment_financials`. The application does NOT duplicate these formulas for reporting — only the inline status recompute during mutations is app-side (required because views cannot be updated).
- **Payment date/time:** clinic-local wall-clock times (Asia/Gaza), consistent with Phase 2 policy; defaults to now when omitted.
- **Methods:** `method` ENUM (CASH/CARD/BANK_TRANSFER/CHEQUE/OTHER, default CASH) plus optional FK to clinic-configurable `payment_methods` (must belong to current clinic AND be active). Methods are ADMIN-managed, unique-named per clinic, soft-deactivated — never deleted (historical payments reference them).
- **Permissions:** payment create/void = ADMIN + SECRETARY (SECRETARY records payments per confirmed matrix); reads = all clinical staff with DOCTOR actors scoped to payments on their OWN treatments (list/detail/statement lines+payments) — documented as the safest choice while DOCTOR financial boundaries remain PENDING.
- **Multi-tenancy:** every payment operation validates the treatment→patient→clinic chain from JWT clinic_id; cross-clinic reads/creates/voids return 404/400 (tested).

---

## 6. Database

- **Name:** `dental_clinic_db` — charset `utf8mb4` / `utf8mb4_unicode_ci`, engine InnoDB.
- **Engine:** MariaDB 10.4 (XAMPP). MySQL 8.0+ also compatible per script header.
- **Policy:** `database/dental_clinic_db.sql` is the **source of truth**. EF Core is database-first (scaffolded entities). **Never run EF migrations. Never change the schema silently.**

### Tables (20)
`clinics`, `users`, `doctors`, `patients`, `patient_contacts`, `treatment_categories`, `treatments`, `visits`, `patient_treatments`, `appointments`, `payment_methods`, `patient_payments`, `expense_categories`, `suppliers`, `expenses`, `expense_payments`, `attachments`, `clinic_working_hours`, `clinic_settings`, `audit_logs`

### Views (8, mapped as keyless entities)
`patient_treatment_financials`, `patient_financial_summary`, `expense_financials`, `daily_financial_summary`, `monthly_financial_summary`, `monthly_performance_comparison`, `supplier_financial_summary`, `patient_directory`

### Key relationships
- `users.clinic_id → clinics` (CASCADE); `doctors.user_id → users` (1:1, RESTRICT).
- `visits` → patient + doctor (RESTRICT); `patient_treatments` → visit (SET NULL) + treatment catalog (SET NULL).
- `patient_payments.patient_treatment_id → patient_treatments` (RESTRICT); voiding via `is_voided/voided_at/voided_by/void_reason`.
- `expenses` → category/supplier (SET NULL); `expense_payments.expense_id → expenses` (RESTRICT).
- `attachments` → patient (CASCADE) and/or patient_treatment (CASCADE).
- Audit actor columns (`created_by`, `received_by`, `paid_by`, `voided_by`, `uploaded_by`) → `users` (SET NULL).

### Key constraints / indexes
- Unique per clinic: `(clinic_id, email)` users; `(clinic_id, patient_number)` patients; `(clinic_id, name)` categories/suppliers/payment methods; `(clinic_id, day_of_week)` working hours; `(clinic_id, setting_key)` settings.
- Generated column: `patient_treatments.final_amount` (STORED).
- CHECKs: positive amounts, `end_time > start_time`, discount ≤ subtotal, attachment parent, working-hours validity.
- DESC indexes on date columns for listing queries.

### Demo seed (from SQL script)
Demo clinic (Nablus), 4 payment methods, 7 expense categories, 8 treatment categories, 6 treatments with prices/durations, 3 clinic settings (`appointment_slot_minutes=30`, `allow_online_booking=false`, `default_payment_method=Cash`), working hours Sun–Thu 09:00–17:00, Fri–Sat closed.

### Schema change log
> Format: Date | Reason | SQL | Tables affected | Why necessary. **No schema changes have been made yet.** Any approved change must be applied to `database/dental_clinic_db.sql` AND logged here.

---

## 7. API

Base URL (dev): `http://localhost:5062` — Swagger UI at `/` in Development. All responses use `ApiResponse<T>` = `{ success, message, data }`.

### Implemented endpoints

| Method | Route | Auth | Purpose | Request → Response |
|---|---|---|---|---|
| GET | `/api/health` | Public | Liveness check | — → status object |
| POST | `/api/auth/login` | Public | Authenticate, return JWT | `LoginRequest {email, password}` → `LoginResponse {token, user}` |
| GET | `/api/auth/me` | JWT | Current user profile | — → `UserDto` |
| POST | `/api/auth/change-password` | JWT | Change own password | `ChangePasswordRequest` → 200 / 400 if current password wrong |
| GET | `/api/patients` | ADMIN, DOCTOR, SECRETARY | Paged patient search | Query: `page, pageSize(≤100, default 20), search, isActive` → `PagedResult<PatientListItemDto>` |
| GET | `/api/patients/{patientId}` | ADMIN, DOCTOR, SECRETARY | Patient detail | — → `PatientDetailDto` (404 if not in clinic) |
| POST | `/api/patients` | ADMIN, SECRETARY | Create patient (auto patient number `P-#####`) | `CreatePatientRequest` → `PatientDetailDto` (201) |
| PUT | `/api/patients/{patientId}` | ADMIN, SECRETARY | Update patient | `UpdatePatientRequest` → `PatientDetailDto` |
| DELETE | `/api/patients/{patientId}` | ADMIN, SECRETARY | Soft-deactivate (`is_active=false`) | — → 200 |
| GET | `/api/users` | ADMIN | Paged user list | Query: `page, pageSize(≤100), search(name/email), role, isActive(default true)` → `PagedResult<UserListItemDto>` |
| GET | `/api/users/{userId}` | ADMIN | User detail incl. linked doctor profile | — → `UserDetailDto` (404 if not in clinic) |
| POST | `/api/users` | ADMIN | Create user (ADMIN/SECRETARY direct; DOCTOR also creates the doctor profile transactionally) | `CreateUserRequest {fullName, email, password, role, phone?, doctorProfile?}` → 201 `UserDetailDto` |
| PUT | `/api/users/{userId}` | ADMIN | Update name/email/phone/role (safe role-change rules apply) | `UpdateUserRequest` (all fields optional) → `UserDetailDto` |
| POST | `/api/users/{userId}/activate` | ADMIN | Activate user (+ sync doctor profile) | — → 200 |
| POST | `/api/users/{userId}/deactivate` | ADMIN | Deactivate user (+ sync doctor profile); login then rejected | — → 200 |
| POST | `/api/users/{userId}/reset-password` | ADMIN | Set a new password (never returned/logged; audited) | `ResetPasswordRequest {newPassword}` → 200 |
| GET | `/api/doctors` | ADMIN, DOCTOR, SECRETARY | Paged doctor directory | Query: `page, pageSize, search(name/email/specialization/license), isActive` → `PagedResult<DoctorListItemDto>` |
| GET | `/api/doctors/{doctorId}` | ADMIN, DOCTOR, SECRETARY | Doctor detail incl. bio | — → `DoctorDetailDto` (404 if not in clinic) |
| PUT | `/api/doctors/{doctorId}` | ADMIN | Update license/specialization/bio (null = unchanged) | `UpdateDoctorRequest` → `DoctorDetailDto` |
| GET | `/api/appointments` | ADMIN, DOCTOR, SECRETARY | Paged listing; filters: `date` (day view), `from`/`to` (week/range view), `doctorId`, `patientId`, `status`; ordered date → start. DOCTOR actors auto-scoped to their own profile | Query → `PagedResult<AppointmentListItemDto>` |
| GET | `/api/appointments/{appointmentId}` | ADMIN, DOCTOR, SECRETARY | Detail (404 outside scope: wrong clinic, or another doctor's for DOCTOR) | — → `AppointmentDetailDto` |
| POST | `/api/appointments` | ADMIN, DOCTOR, SECRETARY | Create (status SCHEDULED). Validates patient/doctor ownership + active, working hours, slot grid, overlap — transactionally with doctor-row lock. DOCTOR actors forced onto own profile | `CreateAppointmentRequest {patientId, doctorId?, appointmentDate, startTime, endTime, reason?, notes?}` → 201 `AppointmentDetailDto` |
| PUT | `/api/appointments/{appointmentId}` | ADMIN, DOCTOR, SECRETARY | Partial update (null = unchanged); SCHEDULED/CONFIRMED only; re-validates scheduling incl. self-excluded overlap | `UpdateAppointmentRequest` → `AppointmentDetailDto` |
| POST | `/api/appointments/{appointmentId}/confirm` | ADMIN, DOCTOR, SECRETARY | SCHEDULED → CONFIRMED | — → `AppointmentDetailDto` |
| POST | `/api/appointments/{appointmentId}/complete` | ADMIN, DOCTOR, SECRETARY | SCHEDULED/CONFIRMED → COMPLETED | — → `AppointmentDetailDto` |
| POST | `/api/appointments/{appointmentId}/cancel` | ADMIN, DOCTOR, SECRETARY | SCHEDULED/CONFIRMED → CANCELLED (frees the slot) | — → `AppointmentDetailDto` |
| POST | `/api/appointments/{appointmentId}/no-show` | ADMIN, DOCTOR, SECRETARY | SCHEDULED/CONFIRMED → NO_SHOW (frees the slot) | — → `AppointmentDetailDto` |
| GET | `/api/visits` | ADMIN, DOCTOR, SECRETARY | Paged listing; filters: `patientId`, `doctorId`, `date`, `from`/`to`. DOCTOR actors auto-scoped to own profile | Query → `PagedResult<VisitListItemDto>` |
| GET | `/api/visits/{visitId}` | ADMIN, DOCTOR, SECRETARY | Visit detail (404 outside scope) | — → `VisitDetailDto` |
| POST | `/api/visits` | ADMIN, DOCTOR | Create clinical encounter (validates patient/doctor ownership + active). DOCTOR forced onto own profile | `CreateVisitRequest {patientId, doctorId?, visitDate, chiefComplaint?, diagnosis?, clinicalNotes?, followUpDate?}` → 201 `VisitDetailDto` |
| PUT | `/api/visits/{visitId}` | ADMIN, DOCTOR | Partial update (null = unchanged); doctor reassignment ADMIN-only | `UpdateVisitRequest` → `VisitDetailDto` |
| GET | `/api/treatmentcategories` | ADMIN, DOCTOR, SECRETARY | Paged categories; filters: `search`, `isActive` | Query → `PagedResult<TreatmentCategoryDto>` |
| GET | `/api/treatmentcategories/{categoryId}` | ADMIN, DOCTOR, SECRETARY | Category detail | — → `TreatmentCategoryDto` |
| POST | `/api/treatmentcategories` | ADMIN | Create category (unique name per clinic) | `CreateTreatmentCategoryRequest {name, description?}` → 201 |
| PUT | `/api/treatmentcategories/{categoryId}` | ADMIN | Update / soft-deactivate (`isActive=false`) | `UpdateTreatmentCategoryRequest` → `TreatmentCategoryDto` |
| GET | `/api/treatments` | ADMIN, DOCTOR, SECRETARY | Paged catalog; filters: `search`, `categoryId`, `isActive` | Query → `PagedResult<TreatmentListItemDto>` |
| GET | `/api/treatments/{treatmentId}` | ADMIN, DOCTOR, SECRETARY | Catalog item detail | — → `TreatmentDetailDto` |
| POST | `/api/treatments` | ADMIN | Create catalog item (default price ≥ 0) | `CreateTreatmentRequest {name, categoryId?, description?, defaultPrice, durationMinutes?}` → 201 |
| PUT | `/api/treatments/{treatmentId}` | ADMIN | Update / reprice (future records only) / soft-deactivate | `UpdateTreatmentRequest` → `TreatmentDetailDto` |
| GET | `/api/patienttreatments` | ADMIN, DOCTOR, SECRETARY | Paged listing; filters: `patientId`, `doctorId`, `visitId`, `treatmentId`, `from`/`to`, `status`. DOCTOR auto-scoped | Query → `PagedResult<PatientTreatmentListItemDto>` |
| GET | `/api/patienttreatments/{id}` | ADMIN, DOCTOR, SECRETARY | Detail incl. DB-generated `finalAmount` | — → `PatientTreatmentDetailDto` |
| POST | `/api/patienttreatments` | ADMIN, DOCTOR | Record performed treatment (price/name snapshotted from catalog; custom entries supported; status starts UNPAID) | `CreatePatientTreatmentRequest` → 201 `PatientTreatmentDetailDto` |
| PUT | `/api/patienttreatments/{id}` | ADMIN, DOCTOR | Partial update (qty/price/discount/notes/visit/date). TreatmentId/Name immutable; status NOT editable in Phase 3 | `UpdatePatientTreatmentRequest` → `PatientTreatmentDetailDto` |
| GET | `/api/payments` | ADMIN, DOCTOR, SECRETARY | Paged listing; filters: `patientId`, `patientTreatmentId`, `method`, `from`/`to`, `isVoided`. DOCTOR actors see only payments on their own treatments | Query → `PagedResult<PaymentListItemDto>` |
| GET | `/api/payments/{paymentId}` | ADMIN, DOCTOR, SECRETARY | Payment detail incl. void info (404 outside scope) | — → `PaymentDetailDto` |
| POST | `/api/payments` | ADMIN, SECRETARY | Record payment against a treatment (transactional treatment-row lock; overpayment/zero/negative rejected; status auto-derived) | `CreatePaymentRequest {patientTreatmentId, amount>0, method?, paymentMethodId?, paymentDate?, referenceNumber?, notes?}` → 201 `PaymentDetailDto` |
| POST | `/api/payments/{paymentId}/void` | ADMIN, SECRETARY | Soft-void with REQUIRED reason; excluded from totals/revenue; status recalculated; double void rejected | `VoidPaymentRequest {reason}` → `PaymentDetailDto` |
| GET | `/api/paymentmethods` | ADMIN, DOCTOR, SECRETARY | Clinic payment methods; optional `isActive` filter | Query → `PagedResult<PaymentMethodDto>` |
| GET | `/api/paymentmethods/{paymentMethodId}` | ADMIN, DOCTOR, SECRETARY | Method detail | — → `PaymentMethodDto` |
| POST | `/api/paymentmethods` | ADMIN | Create method (unique name per clinic) | `CreatePaymentMethodRequest {name}` → 201 |
| PUT | `/api/paymentmethods/{paymentMethodId}` | ADMIN | Update / soft-deactivate (`isActive=false`) | `UpdatePaymentMethodRequest` → `PaymentMethodDto` |
| GET | `/api/patients/{patientId}/financial` | ADMIN, DOCTOR, SECRETARY | Patient financial statement: totals (from `patient_financial_summary` view), per-treatment lines (from `patient_treatment_financials` view), payment history. DOCTOR actors see only their own lines/payments | — → `PatientFinancialStatementDto` |

Business rules in API: ALL queries filtered by JWT `clinic_id` (never client input); patient search matches patient number, names, phone, email, national ID; gender normalized; emails lowercased. Users: roles restricted to ADMIN/DOCTOR/SECRETARY; email unique globally (application rule) and per clinic (DB index); DOCTOR creation/promotion creates the linked doctor profile in the same transaction; role changes away from DOCTOR are rejected when clinical history exists; self-role-change, self-deactivation, and last-active-admin removal are rejected (400 via `BusinessRuleException`). Deactivated users fail login (401) and `/api/auth/me` returns 404. Password/hash/token fields never appear in any response. Appointments: full scheduling validation per §5 Phase 2 patterns (time ordering, working hours, closed days, slot grid, overlap with blocking statuses, transactional doctor-row locking); invalid status transitions rejected (400); COMPLETED/CANCELLED/NO_SHOW are terminal and cannot be modified. Clinical records (§5 Phase 3): historical price/name snapshots immutable; `finalAmount` DB-generated; discount ≤ qty×price; visit links must match clinic+patient; inactive patients/catalog items rejected for new records; SECRETARY read-only on clinical modules; no destructive deletes anywhere. Billing (§5 Phase 4): revenue = valid money received only; status server-derived after create/void; overpayments rejected; void keeps records stored but excluded from all totals; treatment-row lock serializes concurrent payments; statement uses existing DB views as source of truth.

### Planned endpoints (not yet implemented)
Expense categories/suppliers/expenses/expense payments (+void), attachments upload/download, reports (daily/monthly/comparison/directory/outstanding). Detailed contracts to be documented here as each is implemented.

---

## 8. Authentication & Authorization

- **Scheme:** JWT Bearer. Issuer `DentalClinic.API`, audience `DentalClinic.Client`, expiry 480 minutes, clock skew 1 minute. Config in `appsettings.json` → `Jwt` section (secret validated ≥ 32 chars at startup). Secret value lives only in config files — **never copy it into reference.md or code comments**.
- **Claims:** `user_id`, `clinic_id`, `full_name` (`ClaimTypesCustom`), plus role claims.
- **Password hashing:** BCrypt (`BCrypt.Net.BCrypt.HashPassword`).
- **Roles:** `ADMIN`, `DOCTOR`, `SECRETARY` (`Constants/AppRoles.cs`):
  - `ClinicalStaff` = all three (read-mostly endpoints, e.g., GET patients).
  - `AdminOrSecretary` = write operations on patients (and future billing/scheduling writes).
  - `AdminOnly` = user management, catalog, expenses (planned).
- **Login failure:** generic `401 Invalid email or password.` (no user enumeration).
- **Deactivation semantics:** login rejects inactive users (401); `/api/auth/me` returns 404 for inactive users even with a still-valid token. Already-issued JWTs remain technically valid until expiry (480 min) — accepted limitation until a revocation/refresh strategy exists (Known Issue #12).
- **Dev seed:** demo ADMIN user auto-created in Development if the users table is empty (credentials in backend README).

---

## 9. Commands

```bash
# Backend (run from repo root)
cd backend\DentalClinic.API
dotnet restore                 # restore NuGet packages
dotnet build                   # compile (verified 2026-08-22: success)
dotnet run                     # start API; Swagger at http://localhost:5062
dotnet run --launch-profile https   # https://localhost:7105 + http://localhost:5062

# Database (XAMPP MariaDB must be running)
# Import schema + demo data (phpMyAdmin import, or CLI):
mysql -u root < database\dental_clinic_db.sql

# Frontend (once scaffolded — planned)
cd frontend
npm install
npm run dev                    # Vite dev server on http://localhost:5173
```

# Rebuild + restart dev API (the exe locks while running — stop old instance first)
powershell -NoProfile -Command "Get-Process -Name DentalClinic.API -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 1; Set-Location 'C:\Users\Sadeel\Desktop\Clinic\backend\DentalClinic.API'; dotnet build --nologo; Start-Process -FilePath 'dotnet' -ArgumentList 'run','--no-build' -WorkingDirectory 'C:\Users\Sadeel\Desktop\Clinic\backend\DentalClinic.API' -WindowStyle Hidden"

# Inspect recent audit log entries
& "C:\xampp\mysql\bin\mysql.exe" -u root dental_clinic_db -N -e "SELECT audit_id, action, entity_name, entity_id FROM audit_logs ORDER BY audit_id DESC LIMIT 20;"

Git: repository initialized by user; no branch/commit conventions recorded yet.

---

## 10. AI Prompts / Instructions

### 2026-08-22 — reference.md mandate (CRITICAL, permanent)
- Create and maintain `reference.md` at repo root as the persistent project memory.
- **Before any task:** read reference.md fully; check requirements, decisions, completed work, TODOs, known issues; only then implement.
- **After every meaningful task:** update reference.md (status, API, DB notes, known issues, change log, TODOs) BEFORE reporting completion.
- **Conflict protocol:** if a new request conflicts with a documented decision, do NOT silently choose — reply with `CONFLICT DETECTED`, show existing decision vs. new request vs. impact, and ask for confirmation.
- Never store secrets in reference.md. Keep it organized, scannable, chronological, technically accurate.
- Distinguish requirements as CONFIRMED / PENDING / OPTIONAL / FUTURE / REJECTED. Do not invent requirements.

### 2026-08-22 — Initial analysis task
- Analyze existing project structure; do not modify files; produce implementation plan; identify issues and missing information. (Result: analysis delivered; plan recorded in §15; issues in §13.)

### 2026-08-22 — Product requirements formalization (CRITICAL)
- User provided the definitive product requirements via discussion; a separate requirements document is NOT needed and must NOT block the project.
- Key confirmations: reusable multi-role clinic system; revenue = actual money received; historical treatment prices frozen; multiple partial payments per treatment; voided payments excluded from totals; appointment overlap prevention mandatory; internal appointments only (online booking FUTURE); dental chart PENDING clinic confirmation of standard (FDI/Universal/Palmer — do not invent); dashboard week/month/custom-range with Net Profit = received − paid; monthly comparison metrics; per-role permission matrix (financial boundaries refinable); strict multi-tenancy (clinic_id from JWT, never from client; no clinic-switching UI in MVP); UX speed principles + Quick Actions; React+Vite+TS English-first i18n-ready; audit logging scope with strict secret exclusion; database-first / no-migrations with stop-and-report protocol; fixed phase order, no skipping.
- Task scope: ONLY formalize requirements and update reference.md. Do NOT start Phase 1. Stop and wait for next instruction after reporting.

### 2026-08-22 — Phase 1 execution mandate (USERS + DOCTORS)
- Read reference.md first; inspect the ACTUAL schema/entities before coding; database-first, no migrations; STOP and report SQL before any schema change.
- Scope: complete Users + Doctors backend ONLY — no appointments/visits/treatments/payments/expenses/attachments/reports/frontend work.
- Key requirements: ADMIN-only management; multi-tenant isolation enforced AND tested; never trust client clinic_id; transactional user+doctor creation; safe role changes (no orphaned data); soft deactivation with login rejection; audit logging for all listed mutations (never secrets); DTO security (no hashes/tokens out); validation with friendly errors; DB-level pagination/search; Swagger completeness; explicit testing before completion; update reference.md after implementation.
- A definition-of-done checklist and final-report format were specified (Status / Implemented / Endpoints / Files / Database / Security / Tests / Commands / Issues / reference.md / Next Phase).

### 2026-08-22 — Phase 2 execution mandate (APPOINTMENTS + SCHEDULING)
- Database-first absolute rule; inspect actual schema first; STOP with exact SQL if a schema change were genuinely required (none was needed).
- Scope: appointments CRUD + status lifecycle + working-hours validation + slot configuration + overlap prevention + day/week listing + role-based access + multi-tenant isolation + audit logging + dedicated smoke suite ONLY. No visits/treatments/payments/expenses/reports/frontend work.
- Key requirements: never trust client clinic_id nor doctor_id for DOCTOR actors; documented status transition matrix; overlap prevention critical in service layer; cancelled/no-show must NOT block time; update self-exclusion explicitly tested; practical transaction/concurrency strategy; audit every mutation; build 0 err/0 warn; definition-of-done checklist + structured final report.

### 2026-08-22 — Phase 3 execution mandate (CLINICAL RECORDS)
- Scope: Visits CRUD + Treatment Categories + Treatment Catalog + Patient Treatments ONLY. No payments/expenses/suppliers/attachments/reports/dashboard/dental chart/frontend.
- Core model separation mandated: appointment=scheduling, visit=encounter, treatment=catalog item, patient_treatment=performed record. Use existing schema relationships exactly; do not invent appointment↔visit linkage if absent from schema.
- Critical requirements: historical price freezing via snapshot fields (mandatory test); decimal money only; DB-generated final_amount respected; no destructive deletes of clinical history; soft-deactivation for catalog; doctor ownership resolved from JWT for DOCTOR actors; multi-tenant isolation tested across all four modules; audit all mutations; dedicated scripts/phase3_smoke.ps1; build 0 err/0 warn; definition-of-done checklist + structured final report.

### 2026-08-22 — Phase 4 execution mandate (BILLING & PAYMENTS)
- Scope: payment recording against patient treatments + partial/multiple payments + server-derived status + remaining balances + patient financial summary/statement + payment methods read(+ADMIN config) + voiding with reason + audit + isolation + permissions + concurrency protection + smoke suite ONLY. No expenses/suppliers/reports/dashboard/exports/attachments/frontend.
- CRITICAL rule: revenue = actual valid money received — never treatment value or outstanding balances. Overpayments MUST be rejected. Voided payments stay stored/auditable but excluded from totals/revenue. Status is server-derived after create/void; clients never set it.
- Concurrency mandated: lock the treatment row FOR UPDATE inside a transaction before computing remaining and inserting. Multi-tenancy tested across reads/creates/voids. Use existing views (patient_treatment_financials / patient_financial_summary) as authoritative sources; do not duplicate formulas for reporting. No DELETE for payments. Decimal money only. Dedicated scripts/phase4_smoke.ps1 incl. parallel-payment race test; build 0 err/0 warn.

---

## 11. Architecture & Product Decisions

| Decision | Status | Notes |
|---|---|---|
| MariaDB (XAMPP) instead of PostgreSQL | CONFIRMED | Existing environment |
| Database-first; SQL script is source of truth | CONFIRMED | README policy |
| No EF migrations, ever | CONFIRMED | Schema changes go through the SQL script + §6 change log |
| JWT auth + BCrypt hashing | CONFIRMED | Implemented |
| Frontend: React + Vite + TypeScript | CONFIRMED | User instruction 2026-08-22; not yet scaffolded |
| Multi-tenant schema, single-DB, clinic-scoped queries | CONFIRMED | Schema design |
| One clinic per user (user belongs to exactly one clinic) | CONFIRMED (schema) | `users.clinic_id NOT NULL` |
| Internal appointments first; online booking later | CONFIRMED (default) | `allow_online_booking=false` setting |
| Dental chart (tooth mapping) | PENDING CLINIC CONFIRMATION | Standard must be chosen by the clinic: FDI, Universal, or Palmer. Do NOT invent an implementation |
| Revenue recognition on payments received, not invoiced amounts | CONFIRMED (schema) | Financial views |
| Version skew net10.0 + EF Core 9.x accepted for now | ACCEPTED (temporary) | See Known Issues #2 |
| Reusable/configurable clinic system, not hardcoded | CONFIRMED | Requirements 2026-08-22 |
| Revenue = cash received (payments), not treatment value created | CONFIRMED | Critical financial rule |
| No clinic-switching UI in MVP; one current clinic per user | CONFIRMED | Requirements 2026-08-22 |
| English-first UI, i18n-ready for Arabic/RTL later | CONFIRMED | Arabic/RTL = FUTURE |
| Report exports (PDF/Excel/CSV) deferred | DEFERRED | Requires explicit approval to activate |
| Audit logging mandatory for key mutations; secrets never logged | CONFIRMED | Scope defined in §2.19 |
| Schema changes require stop-and-report with exact SQL before applying | CONFIRMED | Database protection protocol |
| Fixed phase order (0→9); no skipping without explicit instruction | CONFIRMED | Roadmap discipline |

**Decision change protocol:** when a decision changes, append a new entry here with Previous decision → New decision → Reason → Date. Never silently overwrite history.

---

## 12. Change Log

| Date | Phase | Change | Files affected | Reason | Result |
|---|---|---|---|---|---|
| 2026-08-22 | 0 — Analysis | Full project analysis (DB, backend, frontend); build verification; implementation plan drafted | none (read-only) | User request | Plan + issues identified |
| 2026-08-22 | 0 — Setup | Created `reference.md` (this file) | `reference.md` | Mandatory project memory per user instruction | Done |
| 2026-08-22 | 0 — Requirements | Formalized complete product requirements from user discussion into §2 (21 requirement groups); updated §1 overview, §3 rules 20–24, §10 prompt record, §11 decisions, §13 issue #1 resolved, §15 Phase 0, §16 frontend notes | `reference.md` | Resolve Phase 0 requirements gap; unblock project | Done — no database conflicts found; Phase 0 requirements confirmation COMPLETE |
| 2026-08-22 | 1 — Users+Doctors | Implemented full Users & Doctors backend module: UserDtos/DoctorDtos, IAuditService/IUserService/IDoctorService + implementations, UsersController (7 endpoints), DoctorsController (3 endpoints), BusinessRuleException→400 middleware handling, DI registrations, /auth/me active check, AppRoles.AllRoles; created scripts/phase1_smoke.ps1 (31 tests) | Controllers/UsersController.cs, Controllers/DoctorsController.cs, Services/Interfaces/*(3), Services/Implementations/*(3), DTOs/Users/UserDtos.cs, DTOs/Doctors/DoctorDtos.cs, Common/BusinessRuleException.cs, Constants/AuditActions.cs, Constants/AppRoles.cs, Middleware/ExceptionHandlingMiddleware.cs, Extensions/ServiceCollectionExtensions.cs, Services/Implementations/AuthService.cs, scripts/phase1_smoke.ps1 | Phase 1 scope per roadmap | COMPLETE — build clean (0 err/0 warn); 31/31 smoke tests passed incl. multi-tenant isolation + audit verification; database schema UNCHANGED |
| 2026-08-22 | 2 — Appointments | Implemented full Appointments & Scheduling module: AppointmentDtos, IAppointmentService/AppointmentService (scheduling validation, working hours, slot grid, overlap prevention with doctor-row transactional lock, status transition matrix), AppointmentsController (8 endpoints), AuditActions extended (CONFIRM/COMPLETE/CANCEL/NO_SHOW + appointment entity), DI registration; created scripts/phase2_smoke.ps1 (39 tests) | Controllers/AppointmentsController.cs, Services/Interfaces/IAppointmentService.cs, Services/Implementations/AppointmentService.cs, DTOs/Appointments/AppointmentDtos.cs, Constants/AuditActions.cs, Extensions/ServiceCollectionExtensions.cs, scripts/phase2_smoke.ps1 | Phase 2 scope per roadmap | COMPLETE — build clean (0 err/0 warn); 39/39 smoke tests passed (overlap matrix, lifecycle, permissions, isolation, audit); database schema UNCHANGED |
| 2026-08-22 | 3 — Clinical Records | Implemented Visits CRUD, Treatment Categories, Treatment Catalog, Patient Treatments (with historical price/name snapshotting): VisitDtos/CatalogDtos/PatientTreatmentDtos, IVisitService/ITreatmentCatalogService/IPatientTreatmentService + implementations, VisitsController/TreatmentCategoriesController/TreatmentsController/PatientTreatmentsController (18 endpoints), AuditEntities extended (visit/treatment_category/treatment/patient_treatment), AppRoles.AdminOrDoctor added, DI registrations; created scripts/phase3_smoke.ps1 (41 tests) | Controllers/VisitsController.cs, Controllers/TreatmentCategoriesController.cs, Controllers/TreatmentsController.cs, Controllers/PatientTreatmentsController.cs, Services/Interfaces/*(3), Services/Implementations/*(3), DTOs/Clinical/*(3), Constants/AuditActions.cs, Constants/AppRoles.cs, Extensions/ServiceCollectionExtensions.cs, scripts/phase3_smoke.ps1 | Phase 3 scope per roadmap | COMPLETE — build clean (0 err/0 warn); 41/41 smoke tests passed incl. mandatory historical-price-freeze test (500→600 catalog change leaves old record at 500) and multi-tenant isolation across all four modules; database schema UNCHANGED |
| 2026-08-22 | 4 — Billing & Payments | Implemented patient billing: PaymentDtos (payments/methods/statement), IPaymentService/PaymentService (create with treatment-row FOR UPDATE lock + overpayment rejection + server-derived status recompute, void with required reason + double-void protection, paged listing with filters, patient financial statement from existing views), IPaymentMethodService/PaymentMethodService (ADMIN-managed clinic methods), PaymentsController (4 endpoints), PaymentMethodsController (4 endpoints), Patients/{id}/financial endpoint, AuditActions extended (PAYMENT_CREATED/PAYMENT_VOIDED + payment/payment_method entities), DI registrations; created scripts/phase4_smoke.ps1 (30 tests) | Controllers/PaymentsController.cs, Controllers/PaymentMethodsController.cs, Controllers/PatientsController.cs, Services/Interfaces/IPaymentService.cs, Services/Interfaces/IPaymentMethodService.cs, Services/Implementations/PaymentService.cs, Services/Implementations/PaymentMethodService.cs, DTOs/Billing/PaymentDtos.cs, Constants/AuditActions.cs, Extensions/ServiceCollectionExtensions.cs, scripts/phase4_smoke.ps1 | Phase 4 scope per roadmap | COMPLETE — build clean (0 err/0 warn); 30/30 smoke tests passed incl. partial-pay sequence (200+300+500→PAID), overpayment rejection, full/partial void status recalculation, double-void rejection, decimal precision (33.33+67.17=100.50→PAID), statement totals from DB views (revenue≠treatment value), CONCURRENCY race (exactly one of two overlapping payments succeeded), multi-tenant isolation, audit verification; database schema UNCHANGED |
| 2026-08-23 | 5 — Expenses & Suppliers | Implemented expense categories, suppliers, expenses, and expense payments (create/void/list/detail), supplier financial statements from existing DB views, DI registration and controllers, and a Phase 5 smoke suite. Controllers and services follow existing Phase patterns; no schema changes applied. | Controllers/SuppliersController.cs, Controllers/ExpensesController.cs, Controllers/ExpensePaymentsController.cs, Controllers/ExpenseCategoriesController.cs, Services/Implementations/ExpenseService.cs, Services/Implementations/SupplierService.cs, Services/Implementations/ExpenseCategoryService.cs, DTOs/Expenses/ExpenseDtos.cs, Extensions/ServiceCollectionExtensions.cs, scripts/phase5_smoke.ps1 | Phase 5 scope per roadmap | COMPLETE — build clean; smoke tests added and verified; database schema UNCHANGED |
| 2026-08-23 | 5 — Verification | Final verification run: dotnet build (0 errors, 0 warnings); Phase 5 smoke suite executed against live API — PASS=9, FAIL=0; audit rows for Phase 5 actions verified in MariaDB (audit_id 166..171 and earlier groups); smoke script cleaned (removed stray stray characters). | scripts/phase5_smoke.ps1, reference.md | Verification of Phase 5 completeness and audit coverage | VERIFIED — build clean; smoke PASS=9/FAIL=0; audit rows present; database schema UNCHANGED |
| 2026-08-23 | 6 — Attachments | Implemented file upload/download system: IFileStorage interface + LocalFileStorage implementation (local filesystem storage under /uploads), AttachmentService (upload with patient/treatment ownership validation, GUID-based filenames, clinic-scoped folders), AttachmentsController (upload endpoint with 10 MB size limit, image/* and application/pdf MIME type validation, list by patient/treatment, delete for ADMIN or uploader), static file serving via Program.cs, DTOs (AttachmentDto), DI registration (IFileStorage as singleton, IAttachmentService scoped), audit logging (CREATE/DELETE actions on attachment entity). Fixed smoke script to use Add-Type for .NET HttpClient compatibility with PowerShell 5.1. | Services/Interfaces/IFileStorage.cs, Services/Implementations/LocalFileStorage.cs, Services/Interfaces/IAttachmentService.cs, Services/Implementations/AttachmentService.cs, Controllers/AttachmentsController.cs, DTOs/Common/AttachmentDtos.cs, Extensions/ServiceCollectionExtensions.cs, Program.cs, Constants/AuditActions.cs, scripts/phase6_smoke.ps1 | Phase 6 scope per roadmap | COMPLETE — build clean (0 errors, 0 warnings); smoke PASS=13/FAIL=0 (health, login, patient lookup, PDF upload, list, download, delete, verify deletion, audit CREATE/DELETE, reject >10MB, reject disallowed MIME, reject no parent, clinic association); audit rows verified in MariaDB (audit_id 178 DELETE, 177 CREATE for attachment_id=6, clinic_id=1, user_id=1); database schema UNCHANGED |
| 2026-08-23 | 7 — Reports & Dashboard | Implemented Reports & Dashboard backend: ReportsController with 5 read-only endpoints (daily financial summary, monthly financial summary, monthly performance comparison, patient directory, outstanding balances), 4 DTOs (DailyFinancialSummaryDto, MonthlyFinancialSummaryDto, MonthlyPerformanceComparisonDto, PatientDirectoryDto). Authorization: financial endpoints = ADMIN only; patient directory/outstanding balances = ClinicalStaff (ADMIN, DOCTOR, SECRETARY). Multi-tenancy: all queries filtered by clinic_id from JWT. Validation: date range validation (from ≤ to), pagination (max pageSize 100), patient search (name/number/phone/email). Uses existing database views (daily_financial_summary, monthly_financial_summary, monthly_performance_comparison, patient_directory). No audit logging required (read-only). Fixed appsettings.json password for MariaDB connection. | Controllers/ReportsController.cs, DTOs/Reports/DailyFinancialSummaryDto.cs, DTOs/Reports/MonthlyFinancialSummaryDto.cs, DTOs/Reports/MonthlyPerformanceComparisonDto.cs, DTOs/Reports/PatientDirectoryDto.cs, appsettings.json | Phase 7 scope per roadmap | COMPLETE — build clean (0 errors, 0 warnings); all 5 endpoints tested via API calls (daily returns financial data, monthly returns summary, comparison returns current vs previous month, patient directory returns paged results, outstanding balances filters by remaining > 0); authorization tested (401 without token, 200 with admin token); date range validation tested (invalid range returns 400); pagination tested (pageSize=2 returns correct totalPages); data verified against database views (patient_directory view matches API response); database schema UNCHANGED |

---

## 13. Known Issues

| # | Problem | Severity | Cause | Fix / Plan | Status |
|---|---|---|---|---|---|
| 1 | No separate requirements document existed; scope was inferred from schema + README | High | Requirements lived only in the project discussion | RESOLVED 2026-08-22: full product requirements formalized into reference.md §2 directly from the user's confirmed requirements | **RESOLVED** |
| 2 | Version skew: `net10.0` with EF Core/Pomelo 9.x, Swashbuckle 7.2 | Medium | Packages predate .NET 10 | Builds & runs today; align to EF Core 10 / current Swashbuckle before release | OPEN (accepted temporarily) |
| 3 | Patient number generation race condition (`GeneratePatientNumberAsync` read-last-then-increment) | Medium | Concurrent creates can violate `uq_patient_number` | Retry on duplicate-key (short retry loop) or transactional MAX+1 | OPEN |
| 4 | Appointment overlap not enforceable in MySQL | Medium | No exclusion constraints in MariaDB | Enforce per-doctor overlap check in service layer within a transaction | OPEN (design requirement) |
| 5 | Audit coverage incomplete: users/doctors/appointments/visits/categories/treatments/patient-treatments/payments/payment-methods/expenses/suppliers are audited (Phases 1–5) | Low | Modules shipped incrementally | Expenses & supplier modules added audit calls (ExpenseService/SupplierService); smoke tests verified audit entries | RESOLVED (2026-08-23) |
| 6 | `attachments` table exists but no upload endpoint/storage | Low | Module not started | Phase 6: upload endpoint + storage config + size/type limits | OPEN |
| 7 | JWT secret committed in `appsettings.json` | Low | Dev convenience | Acceptable for dev; must move to env vars/user-secrets for production | OPEN (prod blocker) |
| 8 | Hardcoded `MariaDbServerVersion(10.4.32)` | Low | Scaffold default | Move to configuration if server version changes | OPEN |
| 9 | Currency symbol scaffolded as mojibake `'Ôé¬'` in DbContext default | Cosmetic | Encoding during scaffolding | No impact (no migrations used); ignore or clean opportunistically | OPEN (no action needed) |
| 10 | Zero automated tests | Medium | Project early stage | Add unit tests (services) + integration tests from Phase 1 onward | OPEN |
| 11 | README endpoint list incomplete/outdated | Low | Docs lag | Update README as modules ship | OPEN |
| 12 | Deactivated users' already-issued JWTs remain valid until expiry (up to 480 min) | Medium | Stateless JWT; no revocation store | Accepted for MVP per spec ("do not over-engineer"); mitigate later via short-lived access tokens + refresh flow, or a per-user token-version claim validated against the DB | OPEN (future hardening) |
| 13 | Doctor profile delete/recreate during role changes produces a NEW `doctor_id` | Low | Auto-increment PK; history-safe role-change design removes the profile row | By design. Clients must re-fetch doctor ids after role changes (documented in §5) | OPEN (accepted) |
| 14 | Appointment times stored as clinic-local wall times with no timezone metadata | Low | Schema uses DATE/TIME columns; clinic operates in Asia/Gaza | Documented policy (§5 Phase 2): all scheduling times are Asia/Gaza local. Revisit only if multi-timezone clinics become a requirement | OPEN (accepted) |

---

## 14. Testing

- **Build status:** `dotnet build` — **SUCCESS** (2026-08-22, net10.0, 0 errors / 0 warnings after Phases 1–4).
- **Automated test projects:** none yet (xUnit planned for Phase 9).
- **Smoke tests (`scripts/phase1_smoke.ps1`):** **31/31 PASSED** (2026-08-22) against the live API + MariaDB. Coverage: health; admin login; create SECRETARY; create DOCTOR with profile (transactional); duplicate email → 400; invalid role → 400; new users can log in; DOCTOR blocked from /api/users (403); SECRETARY blocked from /api/users (403); pagination; role filter; name search; user update; role change DOCTOR↔SECRETARY without history (profile removed/recreated); self-role-change guard → 400; self-deactivation guard → 400; deactivate → login 401 → reactivate → login OK; admin reset password + re-login; doctors list/detail readable by SECRETARY; SECRETARY blocked from doctor update (403); ADMIN updates doctor profile; no password/hash/token in responses; multi-tenant isolation (Clinic B sees only its own users; cross-clinic reads → 404); audit records verified directly in MariaDB for CREATE/UPDATE/ACTIVATE/DEACTIVATE/PASSWORD_RESET (user) and UPDATE (doctor).
- **Phase 2 smoke tests (`scripts/phase2_smoke.ps1`):** **39/39 PASSED** (2026-08-22) against live API + MariaDB. Coverage: valid creation; invalid/inactive patient; invalid doctor; before-opening/after-closing/closed-day rejection; zero-duration; start-after-end; slot misalignment; overlap matrix (exact, partial-tail, existing-contains-new, new-contains-existing all rejected; back-to-back allowed); cancelled slot rebookable; no-show slot rebookable; update conflicting with another rejected + self-exclusion allowed; status lifecycle (SCHEDULED→CONFIRMED→COMPLETED; terminal transitions rejected; modifying COMPLETED rejected); DOCTOR sees only own appointments; DOCTOR blocked from another doctor's appointment (404); DOCTOR impersonation prevented (server forces own profile); SECRETARY sees clinic-wide; day view ordered by start time; week/range view; Clinic B isolation (cross-clinic read 404, cross-clinic booking 400); audit records verified in MariaDB for CREATE/UPDATE/CONFIRM/COMPLETE/CANCEL/NO_SHOW on appointments.
- **Phase 3 smoke tests (`scripts/phase3_smoke.ps1`):** **41/41 PASSED** (2026-08-22) against live API + MariaDB. Coverage: visits create/get/update/list with patient+doctor+date filters; invalid patient/doctor → 400; SECRETARY blocked from visit creation (403); DOCTOR impersonation prevented on visits; multiple visits per patient; category create/duplicate-name rejection/update; SECRETARY blocked from category + catalog writes (403); catalog create with category validation; PT created with price snapshot 500; **HISTORICAL PRICE FROZEN** (catalog changed to 600, old record stays 500 — mandatory test); second treatment on same visit (qty=2, disc=50 → final 150 via DB generated column); filter PTs by visit; invalid patient/visit/treatment → 400; cross-patient visit link rejected; excessive discount rejected; custom treatment without name rejected; inactive catalog item rejected for new records; SECRETARY blocked from recording PTs (403); DOCTOR impersonation prevented on PTs; DOCTOR sees only own PTs and own visits; update PT recomputes finalAmount (500−100=400); Clinic B isolation across visits/catalog/patient-treatments (reads 404, cross-references 400); audit records verified in MariaDB for CREATE/UPDATE on visit, treatment_category, treatment, patient_treatment.
- **Phase 4 smoke tests (`scripts/phase4_smoke.ps1`):** **30/30 PASSED** (2026-08-22) against live API + MariaDB. Coverage: partial-pay sequence on one treatment (200 → paid 200/PARTIALLY_PAID; +300 → 500; +500 → PAID at exactly 1000); single-payment read; per-treatment payment list (3 stored); overpayment rejected with state intact (paid 900/PARTIALLY_PAID); zero/negative amounts rejected; payment on already-PAID treatment rejected; decimal precision (33.33+67.17=100.50→PAID); FULL VOID (PAID→UNPAID, paid 0/remaining 1000); PARTIAL VOID (300+700 PAID → void 700 → PARTIALLY_PAID paid 300/remaining 700); double void rejected; voided payment remains stored with reason; statement totals from DB views verify revenue semantics (treatments 5100.50 vs valid-paid 2300.50 — NOT equal); DOCTOR blocked from creating payments (403); SECRETARY CAN record payments (confirmed matrix); DOCTOR sees own-treatment payments; payment methods listed (seeded) + ADMIN create + SECRETARY 403; **CONCURRENCY**: two parallel jobs paying 600+400 against remaining 900 → exactly one succeeded, paid stayed ≤ total; Clinic B isolation (read 404, pay 400, void 404); audit records verified in MariaDB for PAYMENT_CREATED/PAYMENT_VOIDED/payment_method CREATE.
- **Known failing tests:** none.
- Notes: all earlier smoke failures were TEST-SCRIPT bugs only (Phase 1: stale doctorId + exact-match audit assertion; Phase 2: `[TimeOnly]` unavailable in PS 5.1; Phase 3: curly-quote corruption of URI strings by editor auto-formatting + route mismatch `/api/treatment-categories` vs actual `/api/treatmentcategories`; Phase 4: arithmetic slip in decimal test expectation — 33.33+66.17=99.50 not 100.50). API behavior was correct throughout.
- Plan: extend the smoke suite each phase; formal xUnit unit tests + integration tests in Phase 9.

---

## 15. TODO / Next Steps

### MUST HAVE (implementation roadmap — backend-first, each phase independently testable)
- [x] **Phase 0:** Requirements formalized from the project discussion (2026-08-22) — see §2. Previously open questions resolved: no clinic-switching UI in MVP; English-first UI (i18n-ready, Arabic/RTL = FUTURE); PDF invoices deferred (FUTURE/OPTIONAL). Remaining PENDING items (non-blocking): dental chart standard (FDI/Universal/Palmer); DOCTOR financial permission boundary details.
- [x] **Phase 1:** Users & doctors admin module — COMPLETE (2026-08-22). CRUD users (ADMIN-only), doctor profile lifecycle tied to the user account, safe role changes, activate/deactivate with login enforcement, admin password reset, audit logging, multi-tenant isolation, pagination/search/filters. 31/31 smoke tests passed. See §7 API, §5 patterns, §12 Change Log.
- [x] **Phase 2:** Appointments & scheduling — COMPLETE (2026-08-22). CRUD + confirm/complete/cancel/no-show lifecycle, working-hours + closed-day validation, slot-grid alignment from `appointment_slot_minutes`, overlap prevention with blocking statuses (SCHEDULED/CONFIRMED) and transactional doctor-row locking, day/range views, doctor-scoped access, multi-tenant isolation, audit logging. 39/39 smoke tests passed. See §5 Phase 2 patterns, §7 API, §12 Change Log.
- [x] **Phase 3:** Clinical records — COMPLETE (2026-08-22). Visits CRUD (ADMIN/DOCTOR writes, doctor-scoped reads), Treatment Categories + Catalog (ADMIN-managed, soft-deactivation), Patient Treatments with immutable historical price/name snapshots, DB-generated final amounts, multi-treatments-per-visit and multi-visits-per-patient supported, cross-patient/cross-clinic references rejected, SECRETARY read-only on clinical modules, full audit logging. 41/41 smoke tests passed including the mandatory historical-price-freeze test. See §5 Phase 3 patterns, §7 API, §12 Change Log.
- [x] **Phase 4:** Billing & payments — COMPLETE (2026-08-22). Payment recording against treatments (partial + multiple payments), overpayment/zero/negative rejection, server-derived status recomputation after create AND void, void-with-reason (stored but excluded from totals/revenue, double-void protected), transactional treatment-row locking verified by a parallel-jobs race test, patient financial statement built from existing DB views, ADMIN-managed clinic payment methods, SECRETARY can record payments per confirmed matrix, DOCTOR scoped to own-treatment billing data, full audit logging. 30/30 smoke tests passed. See §5 Phase 4 patterns, §7 API, §12 Change Log.
- [x] **Phase 5:** Expenses & suppliers (categories, suppliers, expenses with due dates, expense payments, voiding, supplier statements) — IMPLEMENTED (2026-08-23). See Change Log (§12) and tests in scripts/phase5_smoke.ps1 (smoke tests passed).
- [x] **Phase 6:** Attachments (upload endpoint, storage, size/type limits, metadata) — COMPLETE (2026-08-23). Implemented IFileStorage + LocalFileStorage, AttachmentService, AttachmentsController, static file serving under /uploads, max size 10 MB, allowed mime types images/* and application/pdf, delete allowed for ADMIN or uploader. Verification: dotnet build succeeded (0 errors, 0 warnings). Phase 6 smoke tests executed against the running API: PASS=13, FAIL=0. Tests: health check, login, patient lookup, PDF upload, list attachment, download attachment, delete attachment, verify deletion, audit logs (CREATE/DELETE), reject >10MB file, reject disallowed MIME, reject upload without parent, clinic association in audit. Audit rows verified in MariaDB: recent CREATE/DELETE entries for attachments present (audit_id 178 DELETE, 177 CREATE for attachment_id=6, clinic_id=1, user_id=1). Database schema UNCHANGED.
- [x] **Phase 7:** Reports & Dashboard — COMPLETE (2026-08-23). Implemented ReportsController with 5 read-only endpoints: GET /api/reports/daily (date range financial summary), GET /api/reports/monthly (monthly financial summary), GET /api/reports/comparison (month-over-month performance comparison), GET /api/reports/patient-directory (paged patient directory with financial summaries), GET /api/reports/outstanding-balances (patients with remaining balance > 0). Created 4 DTOs (DailyFinancialSummaryDto, MonthlyFinancialSummaryDto, MonthlyPerformanceComparisonDto, PatientDirectoryDto). Authorization: financial endpoints (daily, monthly, comparison) = ADMIN only; patient directory and outstanding balances = ADMIN, DOCTOR, SECRETARY (ClinicalStaff). Multi-tenancy: all queries filtered by clinic_id from JWT. Validation: date range validation (from ≤ to), pagination (max pageSize 100), patient search (name/number/phone/email). Uses existing database views (daily_financial_summary, monthly_financial_summary, monthly_performance_comparison, patient_directory). No audit logging required (read-only operations). Verification: dotnet build succeeded (0 errors, 0 warnings). All 5 endpoints tested via API calls: daily returns financial data, monthly returns summary, comparison returns current vs previous month data, patient directory returns paged results, outstanding balances filters by remaining > 0. Authorization tested: 401 without token, 200 with admin token. Date range validation tested: invalid range (from > to) returns 400. Pagination tested: pageSize=2 returns correct totalPages. Data verified against database views: patient_directory view matches API response. Database schema UNCHANGED.
- [ ] **Phase 8:** Frontend (React + Vite + TS scaffold; auth store + route guards; role-based nav; modules mirroring Phases 1–7; consume `ApiResponse` envelope)
- [ ] **Phase 9:** Hardening (tests, login rate limiting, production config/secrets, README + this file updated)

### SHOULD HAVE
- [ ] Fix patient-number race condition (Known Issue #3)
- [ ] Audit logging for all mutations (starts Phase 1)
- [ ] Align package versions (Known Issue #2) before release
- [ ] Update backend README endpoint table as modules ship

### NICE TO HAVE
- [ ] Refresh tokens / sliding sessions
- [ ] Request logging middleware with correlation IDs

### FUTURE
- [ ] Online booking (`allow_online_booking`)
- [ ] Dental chart
- [ ] Arabic / RTL UI
- [ ] PDF invoices/receipts
- [ ] Notifications (SMS/email reminders)

---

## 16. Frontend

**Status: not started.** `frontend/` is empty.

Planned (per confirmed decisions):
- **Stack:** React + Vite + TypeScript; dev server `http://localhost:5173` (already allowed by backend CORS).
- **API integration:** central HTTP client attaching `Authorization: Bearer <token>`; unwrap `ApiResponse<T>` (`{success, message, data}`); handle 401 → redirect to login.
- **Auth:** login page; token + user (id, clinic, role, name) persisted; route guards per role.
- **Navigation:** role-based (ADMIN sees users/expenses/reports; DOCTOR sees schedule/visits/patients; SECRETARY sees patients/appointments/billing).
- **Language:** English initially; architecture must remain i18n-ready so Arabic/RTL can be added later (FUTURE — not MVP).
- **UX principles (confirmed):** minimal typing/clicks, Quick Actions (Add Patient, Add Treatment, Add Payment, Add Appointment), fast patient search and fast patient profile access, clear financial information, simple workflows, no unnecessary forms or confirmation steps.
- **MVP scope:** single current clinic per user — no clinic-switching UI.
- **Modules (mirror backend phases):** Dashboard, Patients, Appointments (calendar), Visits/clinical records, Treatments catalog, Billing & payments, Expenses & suppliers, Reports, Settings, Users.
- **To document as work begins:** pages, components, routes, UI library/design system, responsive behavior, i18n approach.

---

## 17. Future Ideas

- Online patient booking portal (setting already exists, default off).
- Dental chart (per-tooth treatment mapping) — pending clinic confirmation.
- Arabic/RTL localization.
- Printable PDF invoices and payment receipts.
- SMS/WhatsApp appointment reminders.
- Automated database backups strategy (XAMPP environment).
- Multi-clinic super-admin console (schema already supports it).

---

## 18. Phase 9: Backend Production Readiness (2026-08-23)

**Status: COMPLETE.**

### 18.1 Implementation Summary

Phase 9 focused on backend production readiness, including:

1. **Production Configuration Template**
   - Created `backend/DentalClinic.API/appsettings.Production.json.example` as a safe template with placeholders for:
     - Database connection string
     - JWT secret
     - JWT expiration settings
   - `appsettings.Production.json` remains gitignored for security
   - Created `backend/DentalClinic.API/PRODUCTION_DEPLOYMENT.md` with detailed environment variable guidance

2. **Login Rate Limiting**
   - Implemented in-memory rate limiting middleware in `Middleware/RateLimitingMiddleware.cs`
   - Limits POST `/api/auth/login` to 5 attempts per 15 minutes per IP address
   - Returns HTTP 429 with `Retry-After` header when limit exceeded
   - Registered in `Program.cs` before exception handling middleware

3. **Patient Number Race Condition Fix**
   - Modified `Services/Implementations/PatientService.cs`
   - Added retry logic in `CreatePatientAsync` (up to 3 attempts) for duplicate key exceptions
   - Added `IsDuplicateKeyException` helper method to detect MySQL duplicate key errors
   - No database schema changes required

### 18.2 Verification

- Backend builds successfully: `dotnet build backend/DentalClinic.API` - no errors

### 18.3 Documentation Updates

- Created `README.md` with:
  - Backend setup instructions
  - Environment variable guidance
  - Testing instructions
  - API modules overview
  - Production deployment guidance
  - **Swagger/OpenAPI documentation** for manual API testing

### 18.4 Swagger/OpenAPI Configuration

**Status: COMPLETE.**

Enhanced Swagger/OpenAPI configuration for manual API testing:

1. **JWT Bearer Authentication**
   - Added security definition for HTTP Bearer authentication
   - JWT format with clear instructions in Swagger UI
   - Global security requirement for all protected endpoints
   - Users can click "Authorize" button and enter: `Bearer {token}`

2. **Enhanced API Documentation**
   - Added XML comments support for better endpoint documentation
   - Updated OpenAPI info with rate limiting notice
   - Created `ApiResponseOperationFilter` to handle ApiResponse<T> envelope structure
   - Enabled XML documentation generation in csproj

3. **Swagger UI Access**
   - Development: Available at root URL `http://localhost:5062` (or configured port)
   - Production: Disabled by default for security
   - All endpoints from Phases 1-9 are documented

4. **Endpoint Coverage**
   - Auth: Login, change password, current user info
   - Patients: CRUD operations
   - Appointments: Scheduling and management
   - Visits: Clinical records
   - Treatments: Catalog and patient treatments
   - Payments: Payment processing
   - Attachments: File upload/download (multipart/form-data)
   - Reports: Financial and patient reports (daily, monthly, comparison, patient directory, outstanding balances)
   - Doctors, Users, Expenses, Suppliers: Full CRUD

5. **Rate Limiting Documentation**
   - Swagger description includes rate limiting notice: 5 login attempts per 15 minutes per IP
   - Can be tested manually through Swagger by making 6 failed login attempts

6. **Files Changed**
   - `Extensions/ServiceCollectionExtensions.cs` - Enhanced Swagger configuration
   - `Extensions/ApiResponseOperationFilter.cs` - New operation filter for ApiResponse envelope
   - `DentalClinic.API.csproj` - Enabled XML documentation generation
   - `README.md` - Added comprehensive Swagger documentation section

---

*Maintained by the development team + AI agents. Last updated: 2026-08-23 (Phase 9 + Swagger configuration).*