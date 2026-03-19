using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectSaas.Worker.Models;

namespace ProjectSaas.Worker.Persistence.Configurations;

public sealed class OutboxMessageRecordConfiguration : IEntityTypeConfiguration<OutboxMessageRecord>
{
  public void Configure(EntityTypeBuilder<OutboxMessageRecord> builder)
  {
    builder.ToTable("OutboxMessages");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.OrganisationId)
        .IsRequired();

    builder.Property(x => x.EventType)
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(x => x.PayloadJson)
        .IsRequired();

    builder.Property(x => x.OccurredAtUtc)
        .IsRequired();

    builder.Property(x => x.ProcessedAtUtc);

    builder.Property(x => x.RetryCount)
        .IsRequired();

    builder.Property(x => x.LastError)
        .HasMaxLength(4000);

    builder.HasIndex(x => new { x.ProcessedAtUtc, x.OccurredAtUtc });
    builder.HasIndex(x => x.OrganisationId);
  }
}