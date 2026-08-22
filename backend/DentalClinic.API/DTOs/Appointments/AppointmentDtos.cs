using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Appointments;

public class AppointmentListItemDto
{
    public ulong AppointmentId { get; set; }

    public ulong PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public ulong DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Reason { get; set; }
}

public class AppointmentDetailDto : AppointmentListItemDto
{
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class CreateAppointmentRequest
{
    [Required]
    public ulong? PatientId { get; set; }

    /// <summary>
    /// Required for ADMIN/SECRETARY (must belong to the current clinic).
    /// Ignored for DOCTOR users — the authenticated doctor's own profile is used.
    /// </summary>
    public ulong? DoctorId { get; set; }

    [Required]
    public DateOnly? AppointmentDate { get; set; }

    /// <summary>Clinic-local wall-clock time (Asia/Gaza). Must align to the configured slot grid.</summary>
    [Required]
    public TimeOnly? StartTime { get; set; }

    /// <summary>Clinic-local wall-clock time (Asia/Gaza). Must be after StartTime and within working hours.</summary>
    [Required]
    public TimeOnly? EndTime { get; set; }

    [MaxLength(250)]
    public string? Reason { get; set; }

    public string? Notes { get; set; }
}

public class UpdateAppointmentRequest
{
    /// <summary>All fields optional; null leaves the value unchanged.</summary>
    public ulong? PatientId { get; set; }

    /// <summary>Ignored for DOCTOR users (ownership is enforced server-side).</summary>
    public ulong? DoctorId { get; set; }

    public DateOnly? AppointmentDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    [MaxLength(250)]
    public string? Reason { get; set; }

    public string? Notes { get; set; }
}

public class AppointmentSearchQuery
{
    /// <summary>Single-day view (day schedule). Combined with From/To when both provided, Date wins as an exact-day filter.</summary>
    public DateOnly? Date { get; set; }

    /// <summary>Range start (inclusive) for week/range views.</summary>
    public DateOnly? From { get; set; }

    /// <summary>Range end (inclusive) for week/range views.</summary>
    public DateOnly? To { get; set; }

    /// <summary>Filter by doctor. Ignored (overridden) for DOCTOR users, who are always scoped to their own profile.</summary>
    public ulong? DoctorId { get; set; }

    public ulong? PatientId { get; set; }

    /// <summary>Exact status filter: SCHEDULED, CONFIRMED, COMPLETED, CANCELLED, NO_SHOW.</summary>
    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;
}