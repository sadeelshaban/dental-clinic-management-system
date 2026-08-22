using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class PatientDirectory
{
    public ulong PatientId { get; set; }

    public ulong ClinicId { get; set; }

    public string PatientNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public bool? IsActive { get; set; }

    public decimal? TotalTreatments { get; set; }

    public decimal? TotalPaid { get; set; }

    public decimal? TotalRemaining { get; set; }
}
