using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class Appointment
{
    public ulong AppointmentId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong PatientId { get; set; }

    public ulong DoctorId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public ulong? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
