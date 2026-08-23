# Production Deployment Guide

## Environment Variables

For production deployment, use environment variables instead of hardcoding secrets in configuration files.

### 1. Database Connection String

**Environment Variable:** `ConnectionStrings__DentalClinicDb`

**Example:**
```
Server=prod-db-server;Port=3306;Database=dental_clinic_db;User=prod_user;Password=SECURE_PASSWORD;TreatTinyAsBoolean=true;
```

### 2. JWT Secret

**Environment Variable:** `Jwt__Secret`

**Requirements:**
- Must be at least 32 characters long for secure signing
- Use a cryptographically secure random string

**Generate using OpenSSL:**
```bash
openssl rand -base64 32
```

### 3. JWT Expiration

**Environment Variable:** `Jwt__ExpirationMinutes`

**Default:** 480 minutes (8 hours)

## Configuration Files

1. Copy `appsettings.Production.json.example` to `appsettings.Production.json`
2. Replace placeholders with actual values OR use environment variables
3. `appsettings.Production.json` is ignored by git to prevent secrets from being committed

## Security Notes

- Never commit real passwords, tokens, or secrets to version control
- Use different secrets for development, staging, and production environments
- Rotate JWT secrets periodically
- Use strong database passwords
- Enable HTTPS in production
- Review CORS settings for production domains
