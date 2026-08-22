using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class Patient
{
    public ulong PatientId { get; set; }

    public ulong ClinicId { get; set; }

    public string PatientNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string? NationalId { get; set; }

    public string? Address { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string? MedicalAlerts { get; set; }

    public string? Allergies { get; set; }

    public string? Medications { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Notes { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual ICollection<PatientContact> PatientContacts { get; set; } = new List<PatientContact>();

    public virtual ICollection<PatientPayment> PatientPayments { get; set; } = new List<PatientPayment>();

    public virtual ICollection<PatientTreatment> PatientTreatments { get; set; } = new List<PatientTreatment>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
