using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class PatientPayment
{
    public ulong PaymentId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong PatientId { get; set; }

    public ulong PatientTreatmentId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string Method { get; set; } = null!;

    public ulong? PaymentMethodId { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public ulong? ReceivedBy { get; set; }

    public bool IsVoided { get; set; }

    public DateTime? VoidedAt { get; set; }

    public ulong? VoidedBy { get; set; }

    public string? VoidReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual PatientTreatment PatientTreatment { get; set; } = null!;

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual User? ReceivedByNavigation { get; set; }

    public virtual User? VoidedByNavigation { get; set; }
}
