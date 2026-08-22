using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Doctors;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class DoctorService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : IDoctorService
{
    public async Task<PagedResult<DoctorListItemDto>> GetDoctorsAsync(
        ulong clinicId,
        DoctorSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var doctorsQuery = dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.ClinicId == clinicId);

        if (query.IsActive.HasValue)
        {
            doctorsQuery = query.IsActive.Value
                ? doctorsQuery.Where(d => d.IsActive != false)
                : doctorsQuery.Where(d => d.IsActive == false);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            doctorsQuery = doctorsQuery.Where(d =>
                d.User.FullName.Contains(term) ||
                d.User.Email.Contains(term) ||
                (d.Specialization != null && d.Specialization.Contains(term)) ||
                (d.LicenseNumber != null && d.LicenseNumber.Contains(term)));
        }

        var totalCount = await doctorsQuery.CountAsync(cancellationToken);

        var items = await doctorsQuery
            .OrderBy(d => d.User.FullName)
            .ThenBy(d => d.DoctorId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DoctorListItemDto
            {
                DoctorId = d.DoctorId,
                UserId = d.UserId,
                FullName = d.User.FullName,
                Email = d.User.Email,
                Phone = d.User.Phone,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                IsActive = d.IsActive != false
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<DoctorListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<DoctorDetailDto?> GetDoctorByIdAsync(
        ulong clinicId,
        ulong doctorId,
        CancellationToken cancellationToken = default)
    {
        var doctor = await dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.ClinicId == clinicId && d.DoctorId == doctorId)
            .Select(d => new DoctorDetailDto
            {
                DoctorId = d.DoctorId,
                UserId = d.UserId,
                FullName = d.User.FullName,
                Email = d.User.Email,
                Phone = d.User.Phone,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                IsActive = d.IsActive != false,
                Bio = d.Bio,
                CreatedAt = d.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return doctor;
    }

    public async Task<DoctorDetailDto?> UpdateDoctorAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong doctorId,
        UpdateDoctorRequest request,
        CancellationToken cancellationToken = default)
    {
        var doctor = await dbContext.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(
                d => d.ClinicId == clinicId && d.DoctorId == doctorId,
                cancellationToken);

        if (doctor is null)
        {
            return null;
        }

        var oldSnapshot = new
        {
            doctor.LicenseNumber,
            doctor.Specialization,
            doctor.Bio
        };

        if (request.LicenseNumber is not null)
        {
            doctor.LicenseNumber = string.IsNullOrWhiteSpace(request.LicenseNumber)
                ? null
                : request.LicenseNumber.Trim();
        }

        if (request.Specialization is not null)
        {
            doctor.Specialization = string.IsNullOrWhiteSpace(request.Specialization)
                ? null
                : request.Specialization.Trim();
        }

        if (request.Bio is not null)
        {
            doctor.Bio = string.IsNullOrWhiteSpace(request.Bio)
                ? null
                : request.Bio.Trim();
        }

        doctor.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.Doctor,
            entityId: doctor.DoctorId,
            newData: new { doctor.LicenseNumber, doctor.Specialization, doctor.Bio },
            oldData: oldSnapshot);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DoctorDetailDto
        {
            DoctorId = doctor.DoctorId,
            UserId = doctor.UserId,
            FullName = doctor.User.FullName,
            Email = doctor.User.Email,
            Phone = doctor.User.Phone,
            Specialization = doctor.Specialization,
            LicenseNumber = doctor.LicenseNumber,
            IsActive = doctor.IsActive != false,
            Bio = doctor.Bio,
            CreatedAt = doctor.CreatedAt
        };
    }
}