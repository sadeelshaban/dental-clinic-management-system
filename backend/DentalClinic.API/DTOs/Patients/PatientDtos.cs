using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Patients;

public class PatientListItemDto
{
    public ulong PatientId { get; set; }

    public string PatientNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class PatientDetailDto : PatientListItemDto
{
    public string? NationalId { get; set; }

    public string? Address { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string? MedicalAlerts { get; set; }

    public string? Allergies { get; set; }

    public string? Medications { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Notes { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class CreatePatientRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string Gender { get; set; } = "UNKNOWN";

    [MaxLength(100)]
    public string? NationalId { get; set; }

    public string? Address { get; set; }

    [MaxLength(150)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(50)]
    public string? EmergencyContactPhone { get; set; }

    public string? MedicalAlerts { get; set; }

    public string? Allergies { get; set; }

    public string? Medications { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Notes { get; set; }
}

public class UpdatePatientRequest : CreatePatientRequest
{
    public bool? IsActive { get; set; }
}

public class PatientSearchQuery
{
    public string? Search { get; set; }

    public bool? IsActive { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
