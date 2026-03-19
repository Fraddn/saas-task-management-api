using Microsoft.EntityFrameworkCore;
using ProjectSaas.Worker.Models;

namespace ProjectSaas.Worker.Persistence;

public sealed class WorkerDbContext : DbContext
{
  public WorkerDbContext(DbContextOptions<WorkerDbContext> options)
      : base(options)
  {
  }

  public DbSet<OutboxMessageRecord> OutboxMessages => Set<OutboxMessageRecord>();
  public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();
  public DbSet<UserLookup> Users => Set<UserLookup>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkerDbContext).Assembly);
    base.OnModelCreating(modelBuilder);
  }
}