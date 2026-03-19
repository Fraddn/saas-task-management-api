using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectSaas.Worker.Models;

namespace ProjectSaas.Worker.Persistence.Configurations;

public sealed class UserLookupConfiguration : IEntityTypeConfiguration<UserLookup>
{
  public void Configure(EntityTypeBuilder<UserLookup> builder)
  {
    builder.ToTable("users");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasColumnName("Id");

    builder.Property(x => x.OrganisationId)
        .HasColumnName("OrganisationId")
        .IsRequired();

    builder.Property(x => x.Role)
        .HasColumnName("Role")
        .IsRequired();

    builder.Property(x => x.IsDisabled)
        .HasColumnName("IsDisabled")
        .IsRequired();
  }
}