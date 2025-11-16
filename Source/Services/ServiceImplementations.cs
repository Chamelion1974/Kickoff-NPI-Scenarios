using Microsoft.EntityFrameworkCore;
using ShopStewardHub.DigitalTwin.Infrastructure.Database;
using ShopStewardHub.DigitalTwin.Infrastructure.Caching;
using ShopStewardHub.DigitalTwin.Models;
using ShopStewardHub.DigitalTwin.Models.DTOs;

namespace ShopStewardHub.DigitalTwin.Services;

public class MachineService : IMachineService
{
    private readonly MetadataDbContext _dbContext;
    private readonly ICacheService _cache;
    private readonly ITelemetryService _telemetry;

    public MachineService(
        MetadataDbContext dbContext,
        ICacheService cache,
        ITelemetryService telemetry)
    {
        _dbContext = dbContext;
        _cache = cache;
        _telemetry = telemetry;
    }

    public async Task<List<MachineDto>> GetAllMachinesAsync()
    {
        var machines = await _dbContext.Machines
            .Include(m => m.Department)
            .Where(m => m.IsActive)
            .ToListAsync();

        var statuses = await _telemetry.GetCurrentMachineStatusesAsync();

        return machines.Select(m => MapToDto(m, statuses.GetValueOrDefault(m.Id))).ToList();
    }

    public async Task<MachineDto?> GetMachineByIdAsync(Guid id)
    {
        var machine = await _dbContext.Machines
            .Include(m => m.Department)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (machine == null)
            return null;

        var status = await _telemetry.GetCurrentMachineStatusesAsync();
        return MapToDto(machine, status.GetValueOrDefault(id));
    }

    public async Task<MachineStatusDto?> GetMachineStatusAsync(Guid id)
    {
        var statuses = await _telemetry.GetCurrentMachineStatusesAsync();
        return statuses.GetValueOrDefault(id);
    }

    public async Task<List<MachineDto>> GetMachinesByDepartmentAsync(Guid departmentId)
    {
        var machines = await _dbContext.Machines
            .Include(m => m.Department)
            .Where(m => m.DepartmentId == departmentId && m.IsActive)
            .ToListAsync();

        var statuses = await _telemetry.GetCurrentMachineStatusesAsync();

        return machines.Select(m => MapToDto(m, statuses.GetValueOrDefault(m.Id))).ToList();
    }

    private MachineDto MapToDto(Machine machine, MachineStatusDto? status)
    {
        return new MachineDto(
            machine.Id,
            machine.Name,
            machine.Type.ToString(),
            machine.Model,
            machine.Manufacturer,
            new MachineLocationDto(
                machine.DepartmentId,
                machine.Department?.Name ?? "Unknown",
                machine.PositionX,
                machine.PositionY,
                machine.PositionZ,
                machine.Rotation
            ),
            status ?? new MachineStatusDto(
                MachineStatus.Offline.ToString(),
                0,
                null,
                null,
                null,
                DateTime.UtcNow
            ),
            new MachineCapabilities(
                machine.MaxSpindleSpeed,
                machine.MaxFeedRate,
                machine.NumberOfAxes,
                machine.HasToolChanger,
                machine.ToolCapacity
            )
        );
    }
}

public class JobService : IJobService
{
    private readonly MetadataDbContext _dbContext;
    private readonly ICacheService _cache;

    public JobService(MetadataDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<List<JobRoutingDto>> GetActiveJobsAsync()
    {
        var jobs = await _dbContext.JobRoutings
            .Include(j => j.Operations)
            .ThenInclude(o => o.Department)
            .Include(j => j.Operations)
            .ThenInclude(o => o.Machine)
            .Where(j => j.Status == JobStatus.InProgress || j.Status == JobStatus.Released)
            .OrderBy(j => j.Priority)
            .ToListAsync();

        return jobs.Select(MapToDto).ToList();
    }

    public async Task<JobRoutingDto?> GetJobByIdAsync(Guid id)
    {
        var job = await _dbContext.JobRoutings
            .Include(j => j.Operations)
            .ThenInclude(o => o.Department)
            .Include(j => j.Operations)
            .ThenInclude(o => o.Machine)
            .FirstOrDefaultAsync(j => j.Id == id);

        return job == null ? null : MapToDto(job);
    }

    public async Task<List<JobRoutingDto>> GetJobsByStatusAsync(JobStatus status)
    {
        var jobs = await _dbContext.JobRoutings
            .Include(j => j.Operations)
            .ThenInclude(o => o.Department)
            .Include(j => j.Operations)
            .ThenInclude(o => o.Machine)
            .Where(j => j.Status == status)
            .ToListAsync();

        return jobs.Select(MapToDto).ToList();
    }

    private JobRoutingDto MapToDto(JobRouting job)
    {
        var operations = job.Operations.OrderBy(o => o.Sequence).Select(o => new OperationDto(
            o.Id,
            o.Sequence,
            o.OperationCode,
            o.Description,
            new DepartmentRefDto(o.Department.Id, o.Department.Name, o.Department.Type.ToString()),
            o.Machine != null ? new MachineRefDto(o.Machine.Id, o.Machine.Name) : null,
            new TimeEstimates(
                o.EstimatedSetupHours,
                o.EstimatedCycleHours,
                o.EstimatedTotalHours,
                o.ActualSetupHours,
                o.ActualCycleHours,
                o.ActualTotalHours
            ),
            o.Status.ToString(),
            o.StartedAt,
            o.CompletedAt
        )).ToList();

        var completedOps = operations.Count(o => o.Status == "Completed");
        var totalOps = operations.Count;
        var percentComplete = totalOps > 0 ? (float)completedOps / totalOps * 100 : 0;

        var progress = new JobProgressDto(
            percentComplete,
            completedOps,
            totalOps,
            job.EstimatedTotalHours ?? 0,
            job.ActualTotalHours,
            null // TODO: Calculate remaining hours
        );

        return new JobRoutingDto(
            job.Id,
            job.JobNumber,
            job.PartNumber,
            job.Description,
            job.Quantity,
            job.QuantityCompleted,
            job.Priority,
            job.JobType.ToString(),
            job.Status.ToString(),
            job.DueDate,
            operations,
            progress
        );
    }
}

public class WorkflowService : IWorkflowService
{
    private readonly IJobService _jobService;

    public WorkflowService(IJobService jobService)
    {
        _jobService = jobService;
    }

    public async Task<WorkflowVisualizationDto?> GetNPIWorkflowAsync(Guid jobId)
    {
        var job = await _jobService.GetJobByIdAsync(jobId);
        if (job == null || job.JobType != "NPI")
            return null;

        var steps = job.Operations.Select(o => new WorkflowStepDto(
            o.Sequence,
            o.Description ?? o.OperationCode,
            o.Department.Id,
            o.Department.Name,
            o.Estimates.TotalHours,
            o.Estimates.ActualTotalHours,
            o.Status,
            o.StartedAt,
            o.CompletedAt
        )).ToList();

        var timeline = new WorkflowTimeline(
            job.Operations.FirstOrDefault()?.StartedAt ?? DateTime.UtcNow,
            job.CompletedAt,
            DateTime.UtcNow.AddHours(job.Progress.HoursRemaining ?? 0),
            job.Progress.EstimatedTotalHours,
            job.Progress.ActualTotalHours ?? 0,
            job.Progress.HoursRemaining ?? 0
        );

        return new WorkflowVisualizationDto(
            job.Id,
            job.JobNumber,
            job.PartNumber,
            steps,
            timeline
        );
    }

    public async Task<List<WorkflowVisualizationDto>> GetActiveNPIWorkflowsAsync()
    {
        var jobs = await _jobService.GetActiveJobsAsync();
        var npiJobs = jobs.Where(j => j.JobType == "NPI").ToList();

        var workflows = new List<WorkflowVisualizationDto>();
        foreach (var job in npiJobs)
        {
            var workflow = await GetNPIWorkflowAsync(job.Id);
            if (workflow != null)
                workflows.Add(workflow);
        }

        return workflows;
    }
}

public class TelemetryService : ITelemetryService
{
    private readonly ICacheService _cache;
    private readonly Random _random = new(); // For simulation

    public TelemetryService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<Dictionary<Guid, MachineStatusDto>> GetCurrentMachineStatusesAsync()
    {
        // Try to get from cache
        var cached = await _cache.GetAsync<Dictionary<Guid, MachineStatusDto>>("machine_statuses:current");
        if (cached != null)
            return cached;

        // Return empty dictionary if no data
        // In production, this would query the TimescaleDB for latest status
        return new Dictionary<Guid, MachineStatusDto>();
    }

    public async Task RecordMachineStatusAsync(Guid machineId, MachineStatusDto status)
    {
        // Update the current status in cache
        var statuses = await GetCurrentMachineStatusesAsync();
        statuses[machineId] = status;
        await _cache.SetAsync("machine_statuses:current", statuses, TimeSpan.FromSeconds(5));

        // TODO: Also persist to TimescaleDB for historical tracking
    }
}
