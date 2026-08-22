using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Users;

public class UserListItemDto
{
    public ulong UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public bool HasDoctorProfile { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class UserDetailDto : UserListItemDto
{
    public ulong ClinicId { get; set; }

    public DoctorProfileDto? DoctorProfile { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class DoctorProfileDto
{
    public ulong DoctorId { get; set; }

    public string? LicenseNumber { get; set; }

    public string? Specialization { get; set; }

    public string? Bio { get; set; }

    public bool IsActive { get; set; }
}

public class CreateDoctorProfileRequest
{
    [MaxLength(100)]
    public string? LicenseNumber { get; set; }

    [MaxLength(150)]
    public string? Specialization { get; set; }

    public string? Bio { get; set; }
}

public class CreateUserRequest
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Initial password. Minimum 6 characters. Never returned in any response.</summary>
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    /// <summary>One of: ADMIN, DOCTOR, SECRETARY.</summary>
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    /// <summary>Required context when Role is DOCTOR; ignored for other roles.</summary>
    public CreateDoctorProfileRequest? DoctorProfile { get; set; }
}

public class UpdateUserRequest
{
    [MaxLength(150)]
    public string? FullName { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    /// <summary>Optional role change. One of: ADMIN, DOCTOR, SECRETARY.
    /// Changing away from DOCTOR is rejected if the doctor profile has clinical history.</summary>
    [MaxLength(20)]
    public string? Role { get; set; }
}

public class ResetPasswordRequest
{
    /// <summary>New password set by an ADMIN. Minimum 6 characters. Never returned or logged.</summary>
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

public class UserSearchQuery
{
    /// <summary>Matches full name or email.</summary>
    public string? Search { get; set; }

    /// <summary>Filter by exact role (ADMIN, DOCTOR, SECRETARY).</summary>
    public string? Role { get; set; }

    public bool? IsActive { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}