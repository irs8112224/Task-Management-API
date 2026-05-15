using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;
    

    public TasksController(ITaskService taskService,ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = HttpContext.Items["X-Tenant-Id"]?.ToString() ?? string.Empty;
        _logger.LogInformation("Fetching tasks for TenantId: {TenantId}", tenantId);


        var result = await _taskService.GetAllAsync(tenantId);

        _logger.LogInformation("Fetched {Count} tasks", result?.Count() ?? 0);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = HttpContext.Items["X-Tenant-Id"]?.ToString() ?? string.Empty;
        _logger.LogInformation("Fetching task with Id: {TaskId} and TenantId: {TenantId}", id,tenantId);

        var task = await _taskService.GetByIdAsync(id,tenantId);

        if (task == null)
        {
            _logger.LogWarning("Task not found: {TaskId}", id);
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request)
    {
        _logger.LogInformation("Creating new task with title: {Title}", request.Title);
        var tenantId = HttpContext.Items["X-Tenant-Id"]?.ToString() ?? string.Empty;

        var task = await _taskService.CreateAsync(request,tenantId);

        _logger.LogInformation("Task created with Id: {TaskId}", task.Id);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CreateTaskRequest request)
    {
        _logger.LogInformation("Updating task: {TaskId}", id);
        var tenantId = HttpContext.Items["X-Tenant-Id"]?.ToString() ?? string.Empty;

        await _taskService.UpdateAsync(id, request, tenantId);

        _logger.LogInformation("Task updated successfully: {TaskId}", id);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogWarning("Deleting task: {TaskId}", id);
        var tenantId = HttpContext.Items["X-Tenant-Id"]?.ToString() ?? string.Empty;

        await _taskService.DeleteAsync(id, tenantId);

        _logger.LogWarning("Task deleted: {TaskId}", id);

        return NoContent();
    }
}