using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class Doctor
{
    public ulong DoctorId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong UserId { get; set; }

    public string? LicenseNumber { get; set; }

    public string? Specialization { get; set; }

    public string? Bio { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual ICollection<PatientTreatment> PatientTreatments { get; set; } = new List<PatientTreatment>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
