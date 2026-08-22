using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class PatientTreatmentFinancial
{
    public ulong PatientTreatmentId { get; set; }

    public ulong ClinicId { get; set; }

    public ulong PatientId { get; set; }

    public ulong DoctorId { get; set; }

    public DateTime TreatmentDate { get; set; }

    public string TreatmentName { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal? TreatmentTotal { get; set; }

    public decimal? TotalPaid { get; set; }

    public decimal? RemainingBalance { get; set; }
}
