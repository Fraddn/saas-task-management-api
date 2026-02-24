using System;

namespace ProjectSaas.Api.Domain.Entities;

public sealed class Ticket
{
    public Guid Id { get; set; }

    public Guid OrganisationId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "Open";

    public string Priority { get; set; } = "Medium";

    public Guid CreatedByUserId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public int RowVersion { get; set; }
}
