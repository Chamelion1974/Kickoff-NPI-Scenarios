namespace ShopStewardHub.DigitalTwin.Models;

/// <summary>
/// Represents a machine tool or workstation
/// </summary>
public class Machine
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }

    // Identity
    public string Name { get; set; } = string.Empty;
    public string? MachineNumber { get; set; }
    public MachineType Type { get; set; }
    public string? Model { get; set; }
    public string? Manufacturer { get; set; }
    public string? SerialNumber { get; set; }

    // Installation
    public DateTime? InstallationDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }

    // Physical location within department
    public float PositionX { get; set; } = 0.0f;
    public float PositionY { get; set; } = 0.0f;
    public float PositionZ { get; set; } = 0.0f;
    public float Rotation { get; set; } = 0.0f;

    // Capabilities
    public int? MaxSpindleSpeed { get; set; }
    public float? MaxFeedRate { get; set; }
    public float? WorkEnvelopeX { get; set; }
    public float? WorkEnvelopeY { get; set; }
    public float? WorkEnvelopeZ { get; set; }
    public int? NumberOfAxes { get; set; }
    public bool HasToolChanger { get; set; } = false;
    public int? ToolCapacity { get; set; }

    // Configuration
    public bool IsActive { get; set; } = true;
    public bool IsMaintenance { get; set; } = false;
    public string? ControllerType { get; set; }
    public string? ControllerIp { get; set; }

    // Navigation properties
    public Department Department { get; set; } = null!;

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum MachineType
{
    ThreeAxisMill,
    FiveAxisMill,
    TurningCenter,
    MultiAxisLathe,
    BandSaw,
    CAMWorkstation,
    WashingStation,
    DeburrStation
}

public enum MachineStatus
{
    Offline,
    Idle,
    Running,
    Paused,
    Alarm,
    Maintenance
}
