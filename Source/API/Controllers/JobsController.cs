using Microsoft.AspNetCore.Mvc;
using ShopStewardHub.DigitalTwin.Services;
using ShopStewardHub.DigitalTwin.Models;

namespace ShopStewardHub.DigitalTwin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IJobService jobService,
        ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active jobs
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var jobs = await _jobService.GetActiveJobsAsync();
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active jobs");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a specific job by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
                return NotFound(new { error = $"Job {id} not found" });

            return Ok(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job {JobId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get jobs by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        try
        {
            if (!Enum.TryParse<JobStatus>(status, true, out var jobStatus))
                return BadRequest(new { error = $"Invalid status: {status}" });

            var jobs = await _jobService.GetJobsByStatusAsync(jobStatus);
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jobs with status {Status}", status);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
