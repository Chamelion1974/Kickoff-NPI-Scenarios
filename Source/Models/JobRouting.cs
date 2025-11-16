namespace ShopStewardHub.DigitalTwin.Models;

/// <summary>
/// Represents a job/work order with routing through the shop
/// </summary>
public class JobRouting
{
    public Guid Id { get; set; }

    // Job identity
    public string JobNumber { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string? Revision { get; set; }
    public string? Description { get; set; }
    public string? Customer { get; set; }

    // Job details
    public int Quantity { get; set; }
    public int QuantityCompleted { get; set; } = 0;
    public int Priority { get; set; } = 5;
    public JobType JobType { get; set; }

    // Status
    public JobStatus Status { get; set; } = JobStatus.Created;

    // Dates
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Estimates
    public float? EstimatedTotalHours { get; set; }
    public float? ActualTotalHours { get; set; }

    // ITAR flag
    public bool IsItar { get; set; } = false;

    // Navigation properties
    public ICollection<Operation> Operations { get; set; } = new List<Operation>();

    // Metadata
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum JobType
{
    NPI,
    Production,
    Rework,
    Prototype
}

public enum JobStatus
{
    Created,
    Released,
    InProgress,
    OnHold,
    Completed,
    Cancelled
}
