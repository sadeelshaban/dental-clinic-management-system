using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class User
{
    public ulong UserId { get; set; }

    public ulong ClinicId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? Phone { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<ExpensePayment> ExpensePaymentPaidByNavigations { get; set; } = new List<ExpensePayment>();

    public virtual ICollection<ExpensePayment> ExpensePaymentVoidedByNavigations { get; set; } = new List<ExpensePayment>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<PatientPayment> PatientPaymentReceivedByNavigations { get; set; } = new List<PatientPayment>();

    public virtual ICollection<PatientPayment> PatientPaymentVoidedByNavigations { get; set; } = new List<PatientPayment>();

    public virtual ICollection<PatientTreatment> PatientTreatments { get; set; } = new List<PatientTreatment>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
