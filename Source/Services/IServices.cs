using ShopStewardHub.DigitalTwin.Models;
using ShopStewardHub.DigitalTwin.Models.DTOs;

namespace ShopStewardHub.DigitalTwin.Services;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllDepartmentsAsync();
    Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id);
    Task<DepartmentMetrics> GetDepartmentMetricsAsync(Guid id);
}

public interface IMachineService
{
    Task<List<MachineDto>> GetAllMachinesAsync();
    Task<MachineDto?> GetMachineByIdAsync(Guid id);
    Task<MachineStatusDto?> GetMachineStatusAsync(Guid id);
    Task<List<MachineDto>> GetMachinesByDepartmentAsync(Guid departmentId);
}

public interface IJobService
{
    Task<List<JobRoutingDto>> GetActiveJobsAsync();
    Task<JobRoutingDto?> GetJobByIdAsync(Guid id);
    Task<List<JobRoutingDto>> GetJobsByStatusAsync(JobStatus status);
}

public interface IWorkflowService
{
    Task<WorkflowVisualizationDto?> GetNPIWorkflowAsync(Guid jobId);
    Task<List<WorkflowVisualizationDto>> GetActiveNPIWorkflowsAsync();
}

public interface ITelemetryService
{
    Task<Dictionary<Guid, MachineStatusDto>> GetCurrentMachineStatusesAsync();
    Task RecordMachineStatusAsync(Guid machineId, MachineStatusDto status);
}
