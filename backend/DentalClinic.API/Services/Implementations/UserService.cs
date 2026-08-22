using DentalClinic.API.Common;
using DentalClinic.API.Constants;
using DentalClinic.API.Data;
using DentalClinic.API.DTOs.Common;
using DentalClinic.API.DTOs.Users;
using DentalClinic.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.API.Services.Implementations;

public class UserService(
    DentalClinicDbContext dbContext,
    IAuditService auditService) : IUserService
{
    public async Task<PagedResult<UserListItemDto>> GetUsersAsync(
        ulong clinicId,
        UserSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var usersQuery = dbContext.Users
            .AsNoTracking()
            .Where(u => u.ClinicId == clinicId);

        if (query.IsActive.HasValue)
        {
            usersQuery = query.IsActive.Value
                ? usersQuery.Where(u => u.IsActive != false)
                : usersQuery.Where(u => u.IsActive == false);
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = NormalizeRole(query.Role);
            usersQuery = usersQuery.Where(u => u.Role == role);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            usersQuery = usersQuery.Where(u =>
                u.FullName.Contains(term) ||
                u.Email.Contains(term));
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        var items = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItemDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                Phone = u.Phone,
                IsActive = u.IsActive != false,
                HasDoctorProfile = u.Doctor != null,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(
        ulong clinicId,
        ulong userId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Doctor)
            .FirstOrDefaultAsync(
                u => u.ClinicId == clinicId && u.UserId == userId,
                cancellationToken);

        return user is null ? null : MapToDetail(user);
    }

    public async Task<UserDetailDto> CreateUserAsync(
        ulong clinicId,
        ulong actorUserId,
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = NormalizeRole(request.Role);
        var email = request.Email.Trim().ToLowerInvariant();
        var fullName = request.FullName.Trim();

        // Application rule: emails are unique across the whole system because login
        // identifies a user by email alone. The per-clinic unique index remains the
        // final database-level protection.
        var emailTaken = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email.ToLower() == email, cancellationToken);

        if (emailTaken)
        {
            throw new BusinessRuleException("A user with this email already exists.");
        }

        var now = DateTime.UtcNow;

        var user = new Models.User
        {
            ClinicId = clinicId,
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        Models.Doctor? doctor = null;
        if (role == AppRoles.Doctor)
        {
            doctor = new Models.Doctor
            {
                ClinicId = clinicId,
                User = user,
                LicenseNumber = request.DoctorProfile?.LicenseNumber?.Trim(),
                Specialization = request.DoctorProfile?.Specialization?.Trim(),
                Bio = request.DoctorProfile?.Bio?.Trim(),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        // User + doctor profile must be created atomically.
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            dbContext.Users.Add(user);
            if (doctor is not null)
            {
                dbContext.Doctors.Add(doctor);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            auditService.Record(
                actorUserId,
                clinicId,
                AuditActions.Create,
                AuditEntities.User,
                entityId: user.UserId,
                newData: new
                {
                    user.FullName,
                    user.Email,
                    user.Role,
                    DoctorCreated = doctor is not null
                });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new BusinessRuleException(
                "Unable to create the user. The email may already be in use.");
        }

        return MapToDetail(user, doctor);
    }

    public async Task<UserDetailDto?> UpdateUserAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(u => u.Doctor)
            .FirstOrDefaultAsync(
                u => u.ClinicId == clinicId && u.UserId == userId,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        var oldSnapshot = Snapshot(user);

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (!string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(
                        u => u.UserId != userId && u.Email.ToLower() == email,
                        cancellationToken);

                if (emailTaken)
                {
                    throw new BusinessRuleException("A user with this email already exists.");
                }

                user.Email = email;
            }
        }

        if (request.Phone is not null)
        {
            user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var newRole = NormalizeRole(request.Role);
            if (newRole != user.Role)
            {
                if (userId == actorUserId)
                {
                    throw new BusinessRuleException("You cannot change your own role.");
                }

                if (user.Role == AppRoles.Admin && newRole != AppRoles.Admin)
                {
                    await EnsureNotLastActiveAdminAsync(user, cancellationToken);
                }

                await ApplyRoleChangeAsync(user, newRole, cancellationToken);
                user.Role = newRole;
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.Update,
            AuditEntities.User,
            entityId: user.UserId,
            newData: Snapshot(user),
            oldData: oldSnapshot);

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetail(user);
    }

    public async Task<bool> SetUserActiveStatusAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(u => u.Doctor)
            .FirstOrDefaultAsync(
                u => u.ClinicId == clinicId && u.UserId == userId,
                cancellationToken);

        if (user is null)
        {
            return false;
        }

        var currentlyActive = user.IsActive != false;
        if (currentlyActive == isActive)
        {
            return true; // Idempotent; no state change, no audit entry.
        }

        if (!isActive && userId == actorUserId)
        {
            throw new BusinessRuleException("You cannot deactivate your own account.");
        }

        if (!isActive && user.Role == AppRoles.Admin)
        {
            await EnsureNotLastActiveAdminAsync(user, cancellationToken);
        }

        var now = DateTime.UtcNow;
        user.IsActive = isActive;
        user.UpdatedAt = now;

        // Keep the linked doctor profile in sync so an inactive doctor can never
        // appear as available while being unable to authenticate.
        if (user.Doctor is not null)
        {
            user.Doctor.IsActive = isActive;
            user.Doctor.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        auditService.Record(
            actorUserId,
            clinicId,
            isActive ? AuditActions.Activate : AuditActions.Deactivate,
            AuditEntities.User,
            entityId: user.UserId,
            newData: new { user.FullName, user.Email, user.Role, IsActive = isActive });

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(
        ulong clinicId,
        ulong actorUserId,
        ulong userId,
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(
                u => u.ClinicId == clinicId && u.UserId == userId,
                cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        // Deliberately no old/new values: passwords and hashes must never be logged.
        auditService.Record(
            actorUserId,
            clinicId,
            AuditActions.PasswordReset,
            AuditEntities.User,
            entityId: user.UserId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ApplyRoleChangeAsync(
        Models.User user,
        string newRole,
        CancellationToken cancellationToken)
    {
        var becomingDoctor = newRole == AppRoles.Doctor;
        var wasDoctor = user.Doctor is not null || user.Role == AppRoles.Doctor;

        if (becomingDoctor && !wasDoctor)
        {
            var now = DateTime.UtcNow;
            user.Doctor = new Models.Doctor
            {
                ClinicId = user.ClinicId,
                UserId = user.UserId,
                LicenseNumber = null,
                Specialization = null,
                Bio = null,
                IsActive = user.IsActive != false,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
        else if (!becomingDoctor && user.Doctor is not null)
        {
            var hasHistory =
                await dbContext.Visits.AsNoTracking()
                    .AnyAsync(v => v.DoctorId == user.Doctor.DoctorId, cancellationToken)
                || await dbContext.PatientTreatments.AsNoTracking()
                    .AnyAsync(pt => pt.DoctorId == user.Doctor.DoctorId, cancellationToken)
                || await dbContext.Appointments.AsNoTracking()
                    .AnyAsync(a => a.DoctorId == user.Doctor.DoctorId, cancellationToken);

            if (hasHistory)
            {
                throw new BusinessRuleException(
                    "This user has clinical history as a doctor (visits, treatments, or " +
                    "appointments). Changing the role would orphan that history and is not allowed.");
            }

            dbContext.Doctors.Remove(user.Doctor);
        }
    }

    private async Task EnsureNotLastActiveAdminAsync(
        Models.User user,
        CancellationToken cancellationToken)
    {
        var otherActiveAdmins = await dbContext.Users
            .AsNoTracking()
            .CountAsync(
                u => u.ClinicId == user.ClinicId
                     && u.UserId != user.UserId
                     && u.Role == AppRoles.Admin
                     && u.IsActive != false,
                cancellationToken);

        if (otherActiveAdmins == 0)
        {
            throw new BusinessRuleException(
                "Cannot remove the last active administrator of the clinic.");
        }
    }

    private static string NormalizeRole(string? role)
    {
        var normalized = role?.Trim().ToUpperInvariant();
        return normalized switch
        {
            AppRoles.Admin => AppRoles.Admin,
            AppRoles.Doctor => AppRoles.Doctor,
            AppRoles.Secretary => AppRoles.Secretary,
            _ => throw new BusinessRuleException(
                $"Invalid role '{role}'. Allowed roles: {AppRoles.Admin}, {AppRoles.Doctor}, {AppRoles.Secretary}.")
        };
    }

    private static object Snapshot(Models.User user) => new
    {
        user.FullName,
        user.Email,
        user.Phone,
        user.Role,
        HasDoctorProfile = user.Doctor is not null
    };

    private static UserDetailDto MapToDetail(
        Models.User user,
        Models.Doctor? doctor = null)
    {
        doctor ??= user.Doctor;

        return new UserDetailDto
        {
            UserId = user.UserId,
            ClinicId = user.ClinicId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Phone = user.Phone,
            IsActive = user.IsActive != false,
            HasDoctorProfile = doctor is not null,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            DoctorProfile = doctor is null
                ? null
                : new DoctorProfileDto
                {
                    DoctorId = doctor.DoctorId,
                    LicenseNumber = doctor.LicenseNumber,
                    Specialization = doctor.Specialization,
                    Bio = doctor.Bio,
                    IsActive = doctor.IsActive != false
                }
        };
    }
}