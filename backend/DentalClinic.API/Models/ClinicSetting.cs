using System;
using System.Collections.Generic;

namespace DentalClinic.API.Models;

public partial class ClinicSetting
{
    public ulong SettingId { get; set; }

    public ulong ClinicId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;
}
