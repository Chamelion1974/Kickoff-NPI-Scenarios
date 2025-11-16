namespace ShopStewardHub.DigitalTwin.Models.DTOs;

/// <summary>
/// Data Transfer Objects for API responses
/// These are optimized for UE5 consumption
/// </summary>

public record DepartmentDto(
    Guid Id,
    string Name,
    string Type,
    string? Description,
    DepartmentLayout Layout,
    DepartmentMetrics Metrics,
    List<MachineDto> Machines
);

public record DepartmentLayout(
    float SizeX,
    float SizeY,
    float SizeZ,
    float PositionX,
    float PositionY,
    float PositionZ,
    float Rotation
);

public record DepartmentMetrics(
    float Utilization,
    int MachineCount,
    int ActiveMachines,
    int IdleMachines,
    int AlarmMachines,
    int ActiveJobs,
    int QueuedJobs
);

public record MachineDto(
    Guid Id,
    string Name,
    string Type,
    string? Model,
    string? Manufacturer,
    MachineLocationDto Location,
    MachineStatusDto Status,
    MachineCapabilities? Capabilities
);

public record MachineLocationDto(
    Guid DepartmentId,
    string DepartmentName,
    float PositionX,
    float PositionY,
    float PositionZ,
    float Rotation
);

public record MachineStatusDto(
    string Status,
    float UtilizationPercent,
    float? SpindleLoad,
    float? FeedRate,
    JobInfoDto? CurrentJob,
    DateTime LastUpdate
);

public record MachineCapabilities(
    int? MaxSpindleSpeed,
    float? MaxFeedRate,
    int? NumberOfAxes,
    bool HasToolChanger,
    int? ToolCapacity
);

public record JobInfoDto(
    Guid JobId,
    string JobNumber,
    string PartNumber,
    int Quantity,
    int QuantityCompleted,
    float PercentComplete,
    string? CurrentOperation,
    DateTime? EstimatedCompletion
);

public record JobRoutingDto(
    Guid Id,
    string JobNumber,
    string PartNumber,
    string? Description,
    int Quantity,
    int QuantityCompleted,
    int Priority,
    string JobType,
    string Status,
    DateTime? DueDate,
    List<OperationDto> Operations,
    JobProgressDto Progress
);

public record OperationDto(
    Guid Id,
    int Sequence,
    string OperationCode,
    string? Description,
    DepartmentRefDto Department,
    MachineRefDto? Machine,
    TimeEstimates Estimates,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt
);

public record DepartmentRefDto(
    Guid Id,
    string Name,
    string Type
);

public record MachineRefDto(
    Guid Id,
    string Name
);

public record TimeEstimates(
    float SetupHours,
    float CycleHours,
    float TotalHours,
    float? ActualSetupHours,
    float? ActualCycleHours,
    float? ActualTotalHours
);

public record JobProgressDto(
    float PercentComplete,
    int CompletedOperations,
    int TotalOperations,
    float EstimatedTotalHours,
    float? ActualTotalHours,
    float? HoursRemaining
);

// Real-time update messages for WebSocket
public record MachineStatusUpdateMessage(
    Guid MachineId,
    string MachineName,
    string Status,
    float UtilizationPercent,
    float? SpindleLoad,
    DateTime Timestamp
);

public record JobProgressUpdateMessage(
    Guid JobId,
    string JobNumber,
    Guid OperationId,
    float PercentComplete,
    string Status,
    DateTime Timestamp
);

public record AlarmEventMessage(
    Guid MachineId,
    string MachineName,
    string AlarmCode,
    string Severity,
    string Message,
    DateTime Timestamp
);

// Workflow visualization data
public record WorkflowVisualizationDto(
    Guid JobId,
    string JobNumber,
    string PartNumber,
    List<WorkflowStepDto> Steps,
    WorkflowTimeline Timeline
);

public record WorkflowStepDto(
    int Sequence,
    string Description,
    Guid DepartmentId,
    string DepartmentName,
    float EstimatedDuration,
    float? ActualDuration,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt
);

public record WorkflowTimeline(
    DateTime StartTime,
    DateTime? EndTime,
    DateTime EstimatedEndTime,
    float TotalDurationHours,
    float ElapsedHours,
    float RemainingHours
);
