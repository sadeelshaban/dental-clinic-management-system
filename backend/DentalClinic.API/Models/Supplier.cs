using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class Supplier
{
    public ulong SupplierId { get; set; }

    public ulong ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? ContactPerson { get; set; }

    public string? Notes { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
