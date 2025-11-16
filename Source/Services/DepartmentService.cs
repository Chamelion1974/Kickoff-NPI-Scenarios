using Microsoft.EntityFrameworkCore;
using ShopStewardHub.DigitalTwin.Infrastructure.Database;
using ShopStewardHub.DigitalTwin.Infrastructure.Caching;
using ShopStewardHub.DigitalTwin.Models;
using ShopStewardHub.DigitalTwin.Models.DTOs;

namespace ShopStewardHub.DigitalTwin.Services;

public class DepartmentService : IDepartmentService
{
    private readonly MetadataDbContext _dbContext;
    private readonly ICacheService _cache;
    private readonly ITelemetryService _telemetry;

    public DepartmentService(
        MetadataDbContext dbContext,
        ICacheService cache,
        ITelemetryService telemetry)
    {
        _dbContext = dbContext;
        _cache = cache;
        _telemetry = telemetry;
    }

    public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
    {
        // Try cache first
        var cached = await _cache.GetAsync<List<DepartmentDto>>("departments:all");
        if (cached != null)
            return cached;

        var departments = await _dbContext.Departments
            .Include(d => d.Machines)
            .Where(d => d.IsActive)
            .ToListAsync();

        var dtos = new List<DepartmentDto>();
        var machineStatuses = await _telemetry.GetCurrentMachineStatusesAsync();

        foreach (var dept in departments)
        {
            var machineDtos = dept.Machines.Select(m => MapToMachineDto(m, machineStatuses.GetValueOrDefault(m.Id))).ToList();

            var metrics = CalculateDepartmentMetrics(dept, machineStatuses);

            dtos.Add(new DepartmentDto(
                dept.Id,
                dept.Name,
                dept.Type.ToString(),
                dept.Description,
                new DepartmentLayout(
                    dept.SizeX, dept.SizeY, dept.SizeZ,
                    dept.PositionX, dept.PositionY, dept.PositionZ,
                    dept.Rotation
                ),
                metrics,
                machineDtos
            ));
        }

        // Cache for 60 seconds
        await _cache.SetAsync("departments:all", dtos, TimeSpan.FromSeconds(60));

        return dtos;
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id)
    {
        var cacheKey = $"department:{id}";
        var cached = await _cache.GetAsync<DepartmentDto>(cacheKey);
        if (cached != null)
            return cached;

        var dept = await _dbContext.Departments
            .Include(d => d.Machines)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dept == null)
            return null;

        var machineStatuses = await _telemetry.GetCurrentMachineStatusesAsync();
        var machineDtos = dept.Machines.Select(m => MapToMachineDto(m, machineStatuses.GetValueOrDefault(m.Id))).ToList();
        var metrics = CalculateDepartmentMetrics(dept, machineStatuses);

        var dto = new DepartmentDto(
            dept.Id,
            dept.Name,
            dept.Type.ToString(),
            dept.Description,
            new DepartmentLayout(
                dept.SizeX, dept.SizeY, dept.SizeZ,
                dept.PositionX, dept.PositionY, dept.PositionZ,
                dept.Rotation
            ),
            metrics,
            machineDtos
        );

        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromSeconds(10));

        return dto;
    }

    public async Task<DepartmentMetrics> GetDepartmentMetricsAsync(Guid id)
    {
        var dept = await _dbContext.Departments
            .Include(d => d.Machines)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dept == null)
            throw new KeyNotFoundException($"Department {id} not found");

        var machineStatuses = await _telemetry.GetCurrentMachineStatusesAsync();
        return CalculateDepartmentMetrics(dept, machineStatuses);
    }

    private MachineDto MapToMachineDto(Machine machine, MachineStatusDto? status)
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

    private DepartmentMetrics CalculateDepartmentMetrics(Department dept, Dictionary<Guid, MachineStatusDto> statuses)
    {
        var activeMachines = dept.Machines.Count(m => m.IsActive);
        var runningCount = 0;
        var idleCount = 0;
        var alarmCount = 0;
        float totalUtilization = 0;

        foreach (var machine in dept.Machines.Where(m => m.IsActive))
        {
            if (statuses.TryGetValue(machine.Id, out var status))
            {
                totalUtilization += status.UtilizationPercent;

                switch (status.Status)
                {
                    case "Running":
                        runningCount++;
                        break;
                    case "Idle":
                        idleCount++;
                        break;
                    case "Alarm":
                        alarmCount++;
                        break;
                }
            }
        }

        var avgUtilization = activeMachines > 0 ? totalUtilization / activeMachines : 0;

        // TODO: Get actual job counts from operations table
        var activeJobs = 0;
        var queuedJobs = 0;

        return new DepartmentMetrics(
            avgUtilization,
            dept.Machines.Count,
            runningCount,
            idleCount,
            alarmCount,
            activeJobs,
            queuedJobs
        );
    }
}
