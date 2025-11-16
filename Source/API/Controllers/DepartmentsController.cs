using Microsoft.AspNetCore.Mvc;
using ShopStewardHub.DigitalTwin.Services;

namespace ShopStewardHub.DigitalTwin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        IDepartmentService departmentService,
        ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all departments with their current state
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 10)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a specific department by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
                return NotFound(new { error = $"Department {id} not found" });

            return Ok(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department {DepartmentId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get real-time metrics for a department
    /// </summary>
    [HttpGet("{id}/metrics")]
    public async Task<IActionResult> GetMetrics(Guid id)
    {
        try
        {
            var metrics = await _departmentService.GetDepartmentMetricsAsync(id);
            return Ok(metrics);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Department {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics for department {DepartmentId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
