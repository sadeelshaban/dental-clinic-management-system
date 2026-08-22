using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class Visit
{
    public ulong VisitId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong PatientId { get; set; }

    public ulong DoctorId { get; set; }

    public DateTime VisitDate { get; set; }

    public string? ChiefComplaint { get; set; }

    public string? Diagnosis { get; set; }

    public string? ClinicalNotes { get; set; }

    public DateOnly? FollowUpDate { get; set; }

    public ulong? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<PatientTreatment> PatientTreatments { get; set; } = new List<PatientTreatment>();
}
