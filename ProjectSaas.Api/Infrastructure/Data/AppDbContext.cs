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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();

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

            entity.Property(u => u.IsPlatformAdmin)
                  .HasDefaultValue(false);

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

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("refresh_tokens");
            b.HasKey(x => x.Id);

            b.Property(x => x.OrganisationId).HasColumnName("organisation_id");
            b.Property(x => x.UserId).HasColumnName("user_id");

            b.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired();
            b.Property(x => x.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash");

            b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            b.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at_utc");
            b.Property(x => x.RevokedAtUtc).HasColumnName("revoked_at_utc");

            b.HasIndex(x => x.TokenHash).IsUnique();
            b.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
            b.HasIndex(x => new { x.OrganisationId, x.UserId });
        });

        modelBuilder.Entity<SecurityEvent>(b =>
        {
            b.ToTable("security_events");
            b.HasKey(x => x.Id);

            b.Property(x => x.EventType)
                .HasColumnName("event_type")
                .IsRequired();

            b.Property(x => x.UserId)
                .HasColumnName("user_id");

            b.Property(x => x.OrganisationId)
                .HasColumnName("organisation_id");

            b.Property(x => x.FamilyId)
                .HasColumnName("family_id");

            b.Property(x => x.RequestIpAddress)
                .HasColumnName("request_ip_address");

            b.Property(x => x.OccurredAtUtc)
                .HasColumnName("occurred_at_utc")
                .IsRequired();

            b.Property(x => x.MetadataJson)
                .HasColumnName("metadata_json");

            b.HasIndex(x => x.OccurredAtUtc);
            b.HasIndex(x => new { x.EventType, x.OccurredAtUtc });
            b.HasIndex(x => new { x.UserId, x.OccurredAtUtc });
            b.HasIndex(x => new { x.OrganisationId, x.OccurredAtUtc });
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");

            b.HasKey(x => x.Id);

            b.Property(x => x.OrganisationId)
                .IsRequired();

            b.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.PayloadJson)
                .IsRequired();

            b.Property(x => x.OccurredAtUtc)
                .IsRequired();

            b.Property(x => x.ProcessedAtUtc);

            b.Property(x => x.RetryCount)
                .IsRequired()
                .HasDefaultValue(0);

            b.Property(x => x.LastError)
                .HasMaxLength(4000);

            b.HasIndex(x => new { x.ProcessedAtUtc, x.OccurredAtUtc });
            b.HasIndex(x => x.OrganisationId);
        });
    }
}
