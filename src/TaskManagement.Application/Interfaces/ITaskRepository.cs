using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(string TenantId = "");

    Task<TaskItem?> GetByIdAsync(Guid id, string TenantId = "");

    Task AddAsync(TaskItem task);

    Task UpdateAsync(TaskItem task);

    Task SaveChangesAsync();
}