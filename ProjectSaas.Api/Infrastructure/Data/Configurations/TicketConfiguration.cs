using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectSaas.Api.Domain.Entities;

namespace ProjectSaas.Api.Infrastructure.Data.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> b)
    {
        b.ToTable("tickets");

        b.HasKey(x => x.Id);

        b.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.Description)
            .HasMaxLength(4000);

        b.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(x => x.Priority)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(x => x.IsDeleted)
            .IsRequired();

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc).IsRequired();

        // Optimistic concurrency
        b.Property(x => x.RowVersion)
            .IsConcurrencyToken();

        // Tenant-scoped indexes for typical queries
        b.HasIndex(x => new { x.OrganisationId, x.IsDeleted });
        b.HasIndex(x => new { x.OrganisationId, x.Status, x.IsDeleted });
        b.HasIndex(x => new { x.OrganisationId, x.AssignedToUserId, x.IsDeleted });
        b.HasIndex(x => new { x.OrganisationId, x.CreatedByUserId, x.IsDeleted });
    }
}
