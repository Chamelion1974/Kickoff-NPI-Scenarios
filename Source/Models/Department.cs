namespace ShopStewardHub.DigitalTwin.Models;

/// <summary>
/// Represents a department/work area in the shop floor
/// </summary>
public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DepartmentType Type { get; set; }
    public string? Description { get; set; }

    // Physical layout in UE5 coordinates
    public float SizeX { get; set; } = 10.0f;
    public float SizeY { get; set; } = 10.0f;
    public float SizeZ { get; set; } = 3.0f;
    public float PositionX { get; set; } = 0.0f;
    public float PositionY { get; set; } = 0.0f;
    public float PositionZ { get; set; } = 0.0f;
    public float Rotation { get; set; } = 0.0f;

    // Configuration
    public int MaxMachines { get; set; } = 10;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Machine> Machines { get; set; } = new List<Machine>();

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum DepartmentType
{
    CNCMill,
    CNCLathe,
    Programming,
    Saw,
    ShippingReceiving,
    Deburr,
    PartsCleaning,
    FrontOffice,
    ToolCrib
}
