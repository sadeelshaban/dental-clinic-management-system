using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class PatientContact
{
    public ulong ContactId { get; set; }

    public ulong PatientId { get; set; }

    public string Name { get; set; } = null!;

    public string? Relationship { get; set; }

    public string? Phone { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Patient Patient { get; set; } = null!;
}
