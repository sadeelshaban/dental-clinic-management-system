using System.ComponentModel.DataAnnotations;

namespace DentalClinic.API.DTOs.Clinical;

// ---------------------------------------------------------------------------
// Treatment categories
// ---------------------------------------------------------------------------

public class TreatmentCategoryDto
{
    public ulong CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateTreatmentCategoryRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateTreatmentCategoryRequest
{
    /// <summary>All fields optional; null leaves the value unchanged.</summary>
    [MaxLength(150)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    /// <summary>Soft deactivation preferred over deletion (categories may be referenced historically).</summary>
    public bool? IsActive { get; set; }
}

public class TreatmentCategorySearchQuery
{
    /// <summary>Matches category name.</summary>
    public string? Search { get; set; }

    public bool? IsActive { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;
}

// ---------------------------------------------------------------------------
// Treatment catalog
// ---------------------------------------------------------------------------

public class TreatmentListItemDto
{
    public ulong TreatmentId { get; set; }

    public ulong? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Default price for NEW patient treatments only. Historical records keep their own snapshot.</summary>
    public decimal DefaultPrice { get; set; }

    public int? DurationMinutes { get; set; }

    public bool IsActive { get; set; }
}

public class TreatmentDetailDto : TreatmentListItemDto
{
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateTreatmentRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional category; must belong to the current clinic when provided.</summary>
    public ulong? CategoryId { get; set; }

    public string? Description { get; set; }

    [Required]
    [Range(0, 999999999)]
    public decimal? DefaultPrice { get; set; }

    /// <summary>Typical duration in minutes; must be positive when provided.</summary>
    [Range(1, 1440)]
    public int? DurationMinutes { get; set; }
}

public class UpdateTreatmentRequest
{
    /// <summary>All fields optional; null leaves the value unchanged.</summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    public ulong? CategoryId { get; set; }

    public string? Description { get; set; }

    /// <summary>Changing this affects ONLY future patient treatments; historical snapshots are immutable.</summary>
    [Range(0, 999999999)]
    public decimal? DefaultPrice { get; set; }

    [Range(1, 1440)]
    public int? DurationMinutes { get; set; }

    /// <summary>Soft deactivation preferred over deletion (catalog items may be referenced historically).</summary>
    public bool? IsActive { get; set; }
}

public class TreatmentSearchQuery
{
    /// <summary>Matches treatment name.</summary>
    public string? Search { get; set; }

    public ulong? CategoryId { get; set; }

    public bool? IsActive { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;
}