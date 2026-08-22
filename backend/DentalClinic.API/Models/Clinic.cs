using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class Clinic
{
    public ulong ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public string? LegalName { get; set; }

    public string? LogoUrl { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string CurrencySymbol { get; set; } = null!;

    public string Timezone { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<ClinicSetting> ClinicSettings { get; set; } = new List<ClinicSetting>();

    public virtual ICollection<ClinicWorkingHour> ClinicWorkingHours { get; set; } = new List<ClinicWorkingHour>();

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual ICollection<ExpenseCategory> ExpenseCategories { get; set; } = new List<ExpenseCategory>();

    public virtual ICollection<ExpensePayment> ExpensePayments { get; set; } = new List<ExpensePayment>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<PatientPayment> PatientPayments { get; set; } = new List<PatientPayment>();

    public virtual ICollection<PatientTreatment> PatientTreatments { get; set; } = new List<PatientTreatment>();

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();

    public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

    public virtual ICollection<TreatmentCategory> TreatmentCategories { get; set; } = new List<TreatmentCategory>();

    public virtual ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
