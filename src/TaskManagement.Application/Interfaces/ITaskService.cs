using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllAsync(string tenantId = "");

    Task<TaskItem?> GetByIdAsync(Guid id, string tenantId = "");

    Task<TaskItem> CreateAsync(CreateTaskRequest request, string tenantId = "");

    Task UpdateAsync(Guid id, UpdateTaskRequest  request, string tenantId = "");

    Task DeleteAsync(Guid id, string tenantId = "");
}