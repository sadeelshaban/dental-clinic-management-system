using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class ClinicWorkingHour
{
    public ulong WorkingHourId { get; set; }

    public ulong ClinicId { get; set; }

    public sbyte DayOfWeek { get; set; }

    public bool? IsOpen { get; set; }

    public TimeOnly? OpeningTime { get; set; }

    public TimeOnly? ClosingTime { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;
}
