using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
{
}

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<TaskItem>()
            .Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        base.OnModelCreating(modelBuilder);
    }
}