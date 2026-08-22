using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class TreatmentCategory
{
    public ulong CategoryId { get; set; }

    public ulong ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
}
