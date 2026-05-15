using FluentValidation;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IValidator<CreateTaskRequest> _validator;

    public TaskService(
        ITaskRepository repository,
        IValidator<CreateTaskRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<List<TaskItem>> GetAllAsync(string tenantId = "")
    {
        return await _repository.GetAllAsync(tenantId);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id,string tenantId = "")
    {
        return await _repository.GetByIdAsync(id, tenantId);
    }

    public async Task<TaskItem> CreateAsync(CreateTaskRequest request, string tenantId = "")
    {
        await ValidateAsync(request);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedTo = request.AssignedTo,
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId
        };

        await _repository.AddAsync(task);

        await _repository.SaveChangesAsync();

        return task;
    }

    public async Task UpdateAsync(Guid id, CreateTaskRequest request, string tenantId = "")
    {
        await ValidateAsync(request);

        var task = await _repository.GetByIdAsync(id, tenantId);

        if (task == null)
            throw new Exception("Task not found.");

        if (task.Status == TaskItemStatus.Cancelled)
            throw new Exception("Cancelled tasks are read-only.");

        if (task.Status == TaskItemStatus.Completed &&
            request.Status == TaskItemStatus.InProgress)
        {
            throw new Exception(
                "Status cannot move from Completed to InProgress.");
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedTo = request.AssignedTo;
        task.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(task);

        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string tenantId = "")
    {
        var task = await _repository.GetByIdAsync(id, tenantId);

        if (task == null)
            throw new Exception("Task not found.");

        if (task.Status == TaskItemStatus.Completed)
            throw new Exception("Completed tasks cannot be deleted.");

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(task);

        await _repository.SaveChangesAsync();
    }

    private async Task ValidateAsync(CreateTaskRequest request)
    {
        var validationResult =
            await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}