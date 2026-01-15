using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Domain.Entities;

namespace ProjectSaas.Api.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.ToTable("organisations");

            entity.HasKey(o => o.Id);

            entity.Property(o => o.Name)
                  .IsRequired();

            entity.Property(o => o.Slug)
                  .IsRequired();

            entity.HasIndex(o => o.Slug)
                  .IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email)
                  .IsRequired();

            entity.Property(u => u.PasswordHash)
                  .IsRequired();

            entity.HasIndex(u => new { u.OrganisationId, u.Email })
                  .IsUnique();
        });
    }
}
