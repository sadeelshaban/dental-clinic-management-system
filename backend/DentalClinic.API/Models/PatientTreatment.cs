using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class PatientTreatment
{
    public ulong PatientTreatmentId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong PatientId { get; set; }

    public ulong DoctorId { get; set; }

    public ulong? VisitId { get; set; }

    public ulong? TreatmentId { get; set; }

    public string TreatmentName { get; set; } = null!;

    public DateTime TreatmentDate { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal? FinalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public ulong? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<PatientPayment> PatientPayments { get; set; } = new List<PatientPayment>();

    public virtual Treatment? Treatment { get; set; }

    public virtual Visit? Visit { get; set; }
}
