# Dental Clinic Management System

A comprehensive dental clinic management system with multi-tenancy support, built with .NET 10 backend and React frontend.

## Table of Contents

- [Backend Setup](#backend-setup)
- [Environment Variables](#environment-variables)
- [Testing](#testing)
- [API Modules](#api-modules)
- [Production Deployment](#production-deployment)

## Backend Setup

### Prerequisites

- .NET 10 SDK
- MySQL 8.0+ (for development)
- Node.js 18+ (for frontend)

### Backend Installation

1. Navigate to the backend directory:
```bash
cd backend/DentalClinic.API
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Configure the database connection in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DentalClinicDb": "Server=localhost;Database=dentalclinic;User=root;Password=YOUR_PASSWORD;"
  },
  "Jwt": {
    "Secret": "your-secret-key-change-in-production",
    "ExpirationMinutes": 60
  }
}
```

4. Run the application:
```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

## Environment Variables

For production deployment, use environment variables instead of `appsettings.json`:

### Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__DentalClinicDb` | MySQL connection string | `Server=db.example.com;Database=dentalclinic;User=appuser;Password=securepassword;` |
| `Jwt__Secret` | JWT signing secret (minimum 32 characters) | `your-very-secure-random-secret-key-minimum-32-chars` |
| `Jwt__ExpirationMinutes` | JWT token expiration time in minutes | `60` |

### Setting Environment Variables

**Linux/Mac:**
```bash
export ConnectionStrings__DentalClinicDb="Server=db.example.com;Database=dentalclinic;User=appuser;Password=securepassword;"
export Jwt__Secret="your-very-secure-random-secret-key-minimum-32-chars"
export Jwt__ExpirationMinutes="60"
```

**Windows (PowerShell):**
```powershell
$env:ConnectionStrings__DentalClinicDb="Server=db.example.com;Database=dentalclinic;User=appuser;Password=securepassword;"
$env:Jwt__Secret="your-very-secure-random-secret-key-minimum-32-chars"
$env:Jwt__ExpirationMinutes="60"
```

**Windows (Command Prompt):**
```cmd
set ConnectionStrings__DentalClinicDb=Server=db.example.com;Database=dentalclinic;User=appuser;Password=securepassword;
set Jwt__Secret=your-very-secure-random-secret-key-minimum-32-chars
set Jwt__ExpirationMinutes=60
```

### Production Configuration Template

A production configuration template is provided at `backend/DentalClinic.API/appsettings.Production.json.example`. Copy this to `appsettings.Production.json` and fill in your actual values. **Never commit `appsettings.Production.json` to version control.**

For detailed production deployment guidance, see `backend/DentalClinic.API/PRODUCTION_DEPLOYMENT.md`.

## Testing

### Running Tests

The backend includes unit and integration tests using xUnit:

```bash
cd backend/DentalClinic.API.Tests
dotnet test
```

### Test Coverage

The test suite covers:

- **Authentication Tests** (`Services/AuthServiceTests.cs`)
  - Valid credentials login
  - Invalid password rejection
  - Inactive user rejection
  - Non-existent email rejection

- **Patient Service Tests** (`Services/PatientServiceTests.cs`)
  - Unique patient number generation
  - Sequential numbering
  - Multi-tenancy isolation

- **Payment Service Tests** (`Services/PaymentServiceTests.cs`)
  - Zero amount rejection
  - Negative amount rejection
  - Note: Overpayment, partial payment, and full payment tests require a relational database for row locking and are not included in the in-memory test suite

### Test Limitations

Some tests require a relational database provider (MySQL) for features like:
- Row locking (`ExecuteSqlRawAsync`) in PaymentService and AppointmentService
- Transaction management

These tests are excluded from the in-memory test suite and should be run as integration tests against a test database.

## API Modules

The backend is organized into the following modules:

### Authentication Module
- **Endpoints:** `/api/auth/login`, `/api/auth/change-password`, `/api/auth/me`
- **Features:** JWT-based authentication, password hashing with BCrypt, user activation status
- **Security:** Rate limiting on login endpoint (5 attempts per 15 minutes per IP)

### Patients Module
- **Endpoints:** `/api/patients` (CRUD operations)
- **Features:** Patient management, unique patient number generation (P-XXXXX format), multi-tenancy isolation
- **Concurrency:** Retry logic for patient number generation to handle race conditions

### Appointments Module
- **Endpoints:** `/api/appointments` (CRUD operations)
- **Features:** Appointment scheduling, overlap detection, status management (SCHEDULED, COMPLETED, CANCELLED, NO_SHOW)
- **Validation:** Clinic working hours, doctor availability, time slot alignment

### Billing Module
- **Endpoints:** `/api/payments`, `/api/invoices`
- **Features:** Payment processing, invoice generation, treatment status updates
- **Concurrency:** Row locking for payment processing to prevent double payments

### Doctors Module
- **Endpoints:** `/api/doctors` (CRUD operations)
- **Features:** Doctor management, specialization tracking, availability

### Treatments Module
- **Endpoints:** `/api/treatments`, `/api/treatments/categories`
- **Features:** Treatment catalog, pricing, categories, patient treatment records

### Clinics Module
- **Endpoints:** `/api/clinics`
- **Features:** Multi-tenancy support, clinic settings, working hours configuration

### Audit Module
- **Endpoints:** Internal service for audit logging
- **Features:** Tracks all CRUD operations for compliance and debugging

## Production Deployment

### Security Considerations

1. **JWT Secret:** Use a cryptographically secure random string (minimum 32 characters)
2. **Database:** Use strong passwords, SSL/TLS connections, and restricted database users
3. **HTTPS:** Always use HTTPS in production
4. **CORS:** Configure CORS policies to restrict allowed origins
5. **Rate Limiting:** Login endpoint is rate-limited to prevent brute force attacks

### Deployment Steps

1. Set environment variables as described above
2. Configure your production database
3. Build the application:
```bash
dotnet build --configuration Release
```

4. Publish the application:
```bash
dotnet publish --configuration Release --output ./publish
```

5. Deploy to your hosting provider (IIS, Docker, Azure, etc.)

For detailed deployment instructions, see `backend/DentalClinic.API/PRODUCTION_DEPLOYMENT.md`.

## Development

### Database Migrations

The backend uses Entity Framework Core with Pomelo MySQL provider.

To add a new migration:
```bash
dotnet ef migrations add MigrationName
```

To apply migrations:
```bash
dotnet ef database update
```

### API Documentation (Swagger/OpenAPI)

The backend includes Swagger/OpenAPI documentation for API exploration and manual testing.

### Accessing Swagger UI

**Development:**
- Swagger UI is available at the root URL: `http://localhost:5062` (or your configured port)
- This provides an interactive interface to test all API endpoints

**Production:**
- Swagger is disabled by default in production for security
- To enable in production, configure the Swagger endpoint in `appsettings.Production.json`

### Using Swagger for Manual Testing

1. **Open Swagger UI** at `http://localhost:5062` in development
2. **Login** using the `/api/auth/login` endpoint:
   - Click "Try it out"
   - Enter email and password
   - Click "Execute"
   - Copy the `token` value from the response
3. **Authorize** with JWT:
   - Click the "Authorize" button (lock icon) at the top right
   - Enter: `Bearer YOUR_JWT_TOKEN` (replace YOUR_JWT_TOKEN with the actual token)
   - Click "Authorize"
4. **Test Protected Endpoints**:
   - All endpoints now have the authorization header included
   - Click "Try it out" on any endpoint to test it

### Rate Limiting

The login endpoint is rate-limited to prevent brute force attacks:
- **Limit:** 5 login attempts per 15 minutes per IP address
- **Response:** HTTP 429 (Too Many Requests) with `Retry-After` header when exceeded
- You can test this by making 6 failed login attempts in Swagger

### Endpoint Categories

Swagger organizes endpoints by controller:
- **Auth** - Login, change password, current user info
- **Patients** - Patient CRUD operations
- **Appointments** - Appointment scheduling and management
- **Visits** - Clinical visit records
- **Treatments** - Treatment catalog and patient treatments
- **Payments** - Payment processing
- **Attachments** - File upload/download (multipart/form-data)
- **Reports** - Financial and patient reports
- **Doctors** - Doctor management
- **Users** - User management
- **Expenses** - Expense tracking
- **Suppliers** - Supplier management

### Smoke Scripts

Smoke scripts in the `scripts/` directory provide optional automated verification. These are **not** the primary manual verification method - Swagger/OpenAPI is the recommended tool for manual API testing.

## License

[Add your license information here]
