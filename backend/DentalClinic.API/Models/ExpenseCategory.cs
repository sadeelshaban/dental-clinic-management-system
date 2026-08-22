using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class ExpenseCategory
{
    public ulong CategoryId { get; set; }

    public ulong ClinicId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
