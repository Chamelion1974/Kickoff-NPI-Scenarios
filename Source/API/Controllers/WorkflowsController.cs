using Microsoft.AspNetCore.Mvc;
using ShopStewardHub.DigitalTwin.Services;

namespace ShopStewardHub.DigitalTwin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(
        IWorkflowService workflowService,
        ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    /// <summary>
    /// Get NPI workflow visualization data for a job
    /// </summary>
    [HttpGet("npi/{jobId}")]
    public async Task<IActionResult> GetNPIWorkflow(Guid jobId)
    {
        try
        {
            var workflow = await _workflowService.GetNPIWorkflowAsync(jobId);
            if (workflow == null)
                return NotFound(new { error = $"NPI workflow for job {jobId} not found" });

            return Ok(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NPI workflow for job {JobId}", jobId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all active NPI workflows
    /// </summary>
    [HttpGet("npi/active")]
    public async Task<IActionResult> GetActiveNPIWorkflows()
    {
        try
        {
            var workflows = await _workflowService.GetActiveNPIWorkflowsAsync();
            return Ok(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active NPI workflows");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
