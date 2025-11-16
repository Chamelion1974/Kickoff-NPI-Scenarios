using Microsoft.AspNetCore.Mvc;
using ShopStewardHub.DigitalTwin.Services;

namespace ShopStewardHub.DigitalTwin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MachinesController : ControllerBase
{
    private readonly IMachineService _machineService;
    private readonly ILogger<MachinesController> _logger;

    public MachinesController(
        IMachineService machineService,
        ILogger<MachinesController> logger)
    {
        _machineService = machineService;
        _logger = logger;
    }

    /// <summary>
    /// Get all machines
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var machines = await _machineService.GetAllMachinesAsync();
            return Ok(machines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machines");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a specific machine by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var machine = await _machineService.GetMachineByIdAsync(id);
            if (machine == null)
                return NotFound(new { error = $"Machine {id} not found" });

            return Ok(machine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machine {MachineId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get current status for a machine
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        try
        {
            var status = await _machineService.GetMachineStatusAsync(id);
            if (status == null)
                return NotFound(new { error = $"Machine {id} not found or no status available" });

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving status for machine {MachineId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all machines in a department
    /// </summary>
    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetByDepartment(Guid departmentId)
    {
        try
        {
            var machines = await _machineService.GetMachinesByDepartmentAsync(departmentId);
            return Ok(machines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving machines for department {DepartmentId}", departmentId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
