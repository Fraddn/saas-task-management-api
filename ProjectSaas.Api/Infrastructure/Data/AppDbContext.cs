using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Application.Abstractions.Tenancy;
using ProjectSaas.Api.Domain.Entities;

namespace ProjectSaas.Api.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    private readonly ITenantContext? _tenant;

    // This property is used by EF in query filters (must be instance-level).
    public Guid CurrentOrganisationId => _tenant?.OrganisationId ?? Guid.Empty;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantContext? tenant = null)
        : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
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

            entity.Property(u => u.Email)
                  .HasConversion(
                      v => (v ?? string.Empty).Trim().ToLowerInvariant(),
                      v => v
                  );

            entity.HasIndex(u => new { u.OrganisationId, u.Email })
                  .IsUnique();
        });

        // Tenant + soft delete (Tickets)
        modelBuilder.Entity<Ticket>().HasQueryFilter(t =>
            !t.IsDeleted && (CurrentOrganisationId == Guid.Empty || t.OrganisationId == CurrentOrganisationId)
        );
    }
}