using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class PaymentMethod
{
    public ulong PaymentMethodId { get; set; }

    public ulong ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual ICollection<ExpensePayment> ExpensePayments { get; set; } = new List<ExpensePayment>();

    public virtual ICollection<PatientPayment> PatientPayments { get; set; } = new List<PatientPayment>();
}
