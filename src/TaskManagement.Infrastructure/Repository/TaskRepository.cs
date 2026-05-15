using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskItem>> GetAllAsync(string TenantId = "")
    {
        return await _context.TaskItems.Where(x => x.TenantId == TenantId).ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, string TenantId = "")
    {
        return await _context.TaskItems.Where(x => x.TenantId == TenantId).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(TaskItem task)
    {
        await _context.TaskItems.AddAsync(task);
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.TaskItems.Update(task);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}