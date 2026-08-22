using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class Treatment
{
    public ulong TreatmentId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal DefaultPrice { get; set; }

    public int? DurationMinutes { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TreatmentCategory? Category { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual ICollection<PatientTreatment> PatientTreatments { get; set; } = new List<PatientTreatment>();
}
