using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Doctors;

public class DoctorListItemDto
{
    public ulong DoctorId { get; set; }

    public ulong UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Specialization { get; set; }

    public string? LicenseNumber { get; set; }

    public bool IsActive { get; set; }
}

public class DoctorDetailDto : DoctorListItemDto
{
    public string? Bio { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class UpdateDoctorRequest
{
    /// <summary>Null leaves the value unchanged.</summary>
    [MaxLength(100)]
    public string? LicenseNumber { get; set; }

    /// <summary>Null leaves the value unchanged.</summary>
    [MaxLength(150)]
    public string? Specialization { get; set; }

    /// <summary>Null leaves the value unchanged.</summary>
    public string? Bio { get; set; }
}

public class DoctorSearchQuery
{
    /// <summary>Matches doctor name, email, specialization, or license number.</summary>
    public string? Search { get; set; }

    public bool? IsActive { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}