using System;

namespace ProjectSaas.Api.Application.Tickets;

public sealed record TicketListQuery(
    string? Status = null,
    string? Priority = null,
    bool? AssignedToMe = null,
    bool? CreatedByMe = null,
    int Page = 1,
    int PageSize = 20);

public sealed record CreateTicketRequest(
    string Title,
    string? Description,
    string Priority);

public sealed record UpdateTicketRequest(
    string Title,
    string? Description,
    string Status,
    string Priority,
    int RowVersion);

public sealed record AssignTicketRequest(
    Guid AssigneeUserId,
    int RowVersion);

public sealed record CompleteTicketRequest(
    int RowVersion,
    string? ResolutionNote = null);

public sealed record TicketDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    Guid CreatedByUserId,
    Guid? AssignedToUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    int RowVersion);
