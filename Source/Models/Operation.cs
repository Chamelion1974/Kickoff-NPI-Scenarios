namespace ShopStewardHub.DigitalTwin.Models;

/// <summary>
/// Represents a single operation/step in a job routing
/// </summary>
public class Operation
{
    public Guid Id { get; set; }
    public Guid RoutingId { get; set; }

    // Sequencing
    public int Sequence { get; set; }
    public string OperationCode { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Routing
    public Guid DepartmentId { get; set; }
    public Guid? MachineId { get; set; }

    // Estimates
    public float EstimatedSetupHours { get; set; } = 0.0f;
    public float EstimatedCycleHours { get; set; } = 0.0f;
    public float EstimatedTotalHours => EstimatedSetupHours + EstimatedCycleHours;

    // Actuals
    public float? ActualSetupHours { get; set; }
    public float? ActualCycleHours { get; set; }
    public float ActualTotalHours => (ActualSetupHours ?? 0) + (ActualCycleHours ?? 0);

    // Status
    public OperationStatus Status { get; set; } = OperationStatus.Pending;

    // Dates
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Program info
    public string? ProgramNumber { get; set; }
    public string? ProgramPath { get; set; }
    public string? GcodePath { get; set; }

    // Quality
    public bool RequiresFirstArticle { get; set; } = false;
    public bool FirstArticleCompleted { get; set; } = false;
    public bool InspectionRequired { get; set; } = false;

    // Navigation properties
    public JobRouting Routing { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public Machine? Machine { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum OperationStatus
{
    Pending,
    InProgress,
    Completed,
    Skipped
}
