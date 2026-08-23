using DentalClinic.API.Common;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Patients;
using DentalClinic.API.Models;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class PatientService(DentalClinicDbContext dbContext) : IPatientService
{
    public async Task<PagedResult<PatientListItemDto>> GetPatientsAsync(
        ulong clinicId,
        PatientSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var patientsQuery = dbContext.Patients
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId);

        if (query.IsActive.HasValue)
        {
            patientsQuery = query.IsActive.Value
                ? patientsQuery.Where(p => p.IsActive != false)
                : patientsQuery.Where(p => p.IsActive == false);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            patientsQuery = patientsQuery.Where(p =>
                p.PatientNumber.Contains(term) ||
                p.FirstName.Contains(term) ||
                p.LastName.Contains(term) ||
                (p.Phone != null && p.Phone.Contains(term)) ||
                (p.Email != null && p.Email.Contains(term)) ||
                (p.NationalId != null && p.NationalId.Contains(term)));
        }

        var totalCount = await patientsQuery.CountAsync(cancellationToken);

        var items = await patientsQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToListItem(p))
            .ToListAsync(cancellationToken);

        return new PagedResult<PatientListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PatientDetailDto?> GetPatientByIdAsync(
        ulong clinicId,
        ulong patientId,
        CancellationToken cancellationToken = default)
    {
        var patient = await dbContext.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.ClinicId == clinicId && p.PatientId == patientId,
                cancellationToken);

        return patient is null ? null : MapToDetail(patient);
    }

    public async Task<PatientDetailDto> CreatePatientAsync(
        ulong clinicId,
        CreatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;
        
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var patientNumber = await GeneratePatientNumberAsync(clinicId, cancellationToken);

                var patient = new Patient
                {
                    ClinicId = clinicId,
                    PatientNumber = patientNumber,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Phone = request.Phone?.Trim(),
                    Email = request.Email?.Trim().ToLowerInvariant(),
                    DateOfBirth = request.DateOfBirth,
                    Gender = NormalizeGender(request.Gender),
                    NationalId = request.NationalId?.Trim(),
                    Address = request.Address?.Trim(),
                    EmergencyContactName = request.EmergencyContactName?.Trim(),
                    EmergencyContactPhone = request.EmergencyContactPhone?.Trim(),
                    MedicalAlerts = request.MedicalAlerts?.Trim(),
                    Allergies = request.Allergies?.Trim(),
                    Medications = request.Medications?.Trim(),
                    MedicalHistory = request.MedicalHistory?.Trim(),
                    Notes = request.Notes?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                dbContext.Patients.Add(patient);
                await dbContext.SaveChangesAsync(cancellationToken);

                return MapToDetail(patient);
            }
            catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
            {
                // Duplicate key error - retry with a new number
                if (attempt == maxRetries - 1)
                {
                    throw new BusinessRuleException("Unable to generate a unique patient number. Please try again.");
                }
                // Continue to next retry
            }
        }

        throw new BusinessRuleException("Unable to generate a unique patient number. Please try again.");
    }

    public async Task<PatientDetailDto?> UpdatePatientAsync(
        ulong clinicId,
        ulong patientId,
        UpdatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var patient = await dbContext.Patients
            .FirstOrDefaultAsync(
                p => p.ClinicId == clinicId && p.PatientId == patientId,
                cancellationToken);

        if (patient is null)
        {
            return null;
        }

        patient.FirstName = request.FirstName.Trim();
        patient.LastName = request.LastName.Trim();
        patient.Phone = request.Phone?.Trim();
        patient.Email = request.Email?.Trim().ToLowerInvariant();
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = NormalizeGender(request.Gender);
        patient.NationalId = request.NationalId?.Trim();
        patient.Address = request.Address?.Trim();
        patient.EmergencyContactName = request.EmergencyContactName?.Trim();
        patient.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
        patient.MedicalAlerts = request.MedicalAlerts?.Trim();
        patient.Allergies = request.Allergies?.Trim();
        patient.Medications = request.Medications?.Trim();
        patient.MedicalHistory = request.MedicalHistory?.Trim();
        patient.Notes = request.Notes?.Trim();

        if (request.IsActive.HasValue)
        {
            patient.IsActive = request.IsActive.Value;
        }

        patient.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetail(patient);
    }

    public async Task<bool> DeactivatePatientAsync(
        ulong clinicId,
        ulong patientId,
        CancellationToken cancellationToken = default)
    {
        var patient = await dbContext.Patients
            .FirstOrDefaultAsync(
                p => p.ClinicId == clinicId && p.PatientId == patientId,
                cancellationToken);

        if (patient is null)
        {
            return false;
        }

        patient.IsActive = false;
        patient.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<string> GeneratePatientNumberAsync(
        ulong clinicId,
        CancellationToken cancellationToken)
    {
        var lastNumber = await dbContext.Patients
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId)
            .OrderByDescending(p => p.PatientId)
            .Select(p => p.PatientNumber)
            .FirstOrDefaultAsync(cancellationToken);

        string newNumber;
        
        if (lastNumber is null || !lastNumber.StartsWith('P'))
        {
            newNumber = "P-00001";
        }
        else
        {
            var numericPart = lastNumber.Split('-').LastOrDefault() ?? "0";
            if (int.TryParse(numericPart, out var sequence))
            {
                newNumber = $"P-{(sequence + 1):D5}";
            }
            else
            {
                newNumber = $"P-{DateTime.UtcNow:yyyyMMddHHmmss}";
            }
        }

        return newNumber;
    }

    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        // Check if the exception is due to a duplicate key constraint violation
        return ex.InnerException != null && 
               (ex.InnerException.Message.Contains("Duplicate entry") ||
                ex.InnerException.Message.Contains("uq_patient_number") ||
                ex.InnerException.Message.Contains("PRIMARY"));
    }

    private static string NormalizeGender(string? gender) =>
        gender?.Trim().ToUpperInvariant() switch
        {
            "MALE" => "MALE",
            "FEMALE" => "FEMALE",
            "OTHER" => "OTHER",
            _ => "UNKNOWN"
        };

    private static PatientListItemDto MapToListItem(Patient patient) =>
        new()
        {
            PatientId = patient.PatientId,
            PatientNumber = patient.PatientNumber,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            FullName = $"{patient.FirstName} {patient.LastName}",
            Phone = patient.Phone,
            Email = patient.Email,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            IsActive = patient.IsActive != false,
            CreatedAt = patient.CreatedAt
        };

    private static PatientDetailDto MapToDetail(Patient patient) =>
        new()
        {
            PatientId = patient.PatientId,
            PatientNumber = patient.PatientNumber,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            FullName = $"{patient.FirstName} {patient.LastName}",
            Phone = patient.Phone,
            Email = patient.Email,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            IsActive = patient.IsActive != false,
            CreatedAt = patient.CreatedAt,
            NationalId = patient.NationalId,
            Address = patient.Address,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            MedicalAlerts = patient.MedicalAlerts,
            Allergies = patient.Allergies,
            Medications = patient.Medications,
            MedicalHistory = patient.MedicalHistory,
            Notes = patient.Notes,
            UpdatedAt = patient.UpdatedAt
        };
}
