using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Application.Abstractions.Tenancy;
using ProjectSaas.Api.Application.Exceptions;
using ProjectSaas.Api.Domain.Entities;
using ProjectSaas.Api.Infrastructure.Data;

namespace ProjectSaas.Api.Application.Tickets;

public sealed class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public TicketService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<TicketDto>> GetListAsync(TicketListQuery query, CancellationToken ct)
    {
        var organisationId = _tenant.OrganisationId;
        var userId = _tenant.UserId;
        var role = _tenant.Role;

        var q = _db.Tickets
            .AsNoTracking()
            .Where(t => t.OrganisationId == organisationId && !t.IsDeleted);

        var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            q = q.Where(t => t.CreatedByUserId == userId || t.AssignedToUserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(t => t.Status == query.Status);

        if (!string.IsNullOrWhiteSpace(query.Priority))
            q = q.Where(t => t.Priority == query.Priority);

        if (query.AssignedToMe == true)
            q = q.Where(t => t.AssignedToUserId == userId);

        if (query.CreatedByMe == true)
            q = q.Where(t => t.CreatedByUserId == userId);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        var result = await q
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.CreatedByUserId,
                t.AssignedToUserId,
                t.CreatedAtUtc,
                t.UpdatedAtUtc,
                t.RowVersion))
            .ToListAsync(ct);

        return result;
    }

    public async Task<TicketDto> GetByIdAsync(Guid ticketId, CancellationToken ct)
    {
        var organisationId = _tenant.OrganisationId;
        var userId = _tenant.UserId;
        var role = _tenant.Role;

        var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        var q = _db.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId && t.OrganisationId == organisationId && !t.IsDeleted);

        if (!isAdmin)
        {
            q = q.Where(t => t.CreatedByUserId == userId || t.AssignedToUserId == userId);
        }

        var ticket = await q.SingleOrDefaultAsync(ct);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        return new TicketDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedByUserId,
            ticket.AssignedToUserId,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.RowVersion);
    }

    public async Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var priority = request.Priority.Trim();

        if (!string.Equals(priority, "Low", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(priority, "Medium", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(priority, "High", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Priority must be Low, Medium, or High.");
        }

        priority =
            string.Equals(priority, "Low", StringComparison.OrdinalIgnoreCase) ? "Low" :
            string.Equals(priority, "Medium", StringComparison.OrdinalIgnoreCase) ? "Medium" :
            "High";

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            OrganisationId = _tenant.OrganisationId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = "Open",
            Priority = priority,
            CreatedByUserId = _tenant.UserId,
            AssignedToUserId = null,
            IsDeleted = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            RowVersion = 1
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(ct);

        return new TicketDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedByUserId,
            ticket.AssignedToUserId,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.RowVersion);
    }

    public async Task<TicketDto> UpdateAsync(Guid ticketId, UpdateTicketRequest request, CancellationToken ct)
    {
        var organisationId = _tenant.OrganisationId;
        var userId = _tenant.UserId;
        var role = _tenant.Role;

        var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        var q = _db.Tickets
            .Where(t => t.Id == ticketId && t.OrganisationId == organisationId && !t.IsDeleted);

        if (!isAdmin)
        {
            q = q.Where(t => t.CreatedByUserId == userId || t.AssignedToUserId == userId);
        }

        var ticket = await q.SingleOrDefaultAsync(ct);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        if (ticket.RowVersion != request.RowVersion)
            throw new ConcurrencyConflictException("Concurrency conflict. Please refresh and retry.");

        ticket.Title = request.Title.Trim();
        ticket.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        ticket.Status = request.Status.Trim();

        var priority = request.Priority.Trim();

        if (!string.Equals(priority, "Low", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(priority, "Medium", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(priority, "High", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Priority must be Low, Medium, or High.");
        }

        ticket.Priority =
            string.Equals(priority, "Low", StringComparison.OrdinalIgnoreCase) ? "Low" :
            string.Equals(priority, "Medium", StringComparison.OrdinalIgnoreCase) ? "Medium" :
            "High";

        ticket.UpdatedAtUtc = DateTimeOffset.UtcNow;
        ticket.RowVersion += 1;

        await _db.SaveChangesAsync(ct);

        return new TicketDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedByUserId,
            ticket.AssignedToUserId,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.RowVersion);
    }

    public async Task<TicketDto> AssignAsync(Guid ticketId, AssignTicketRequest request, CancellationToken ct)
    {
        var isAdmin = string.Equals(_tenant.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
            throw new ForbiddenException("Admin role required.");

        var organisationId = _tenant.OrganisationId;

        var assigneeExistsInTenant = await _db.Users
            .AnyAsync(u => u.Id == request.AssigneeUserId && u.OrganisationId == organisationId, ct);

        if (!assigneeExistsInTenant)
            throw new ArgumentException("Invalid assignee.");

        var ticket = await _db.Tickets
            .SingleOrDefaultAsync(t => t.Id == ticketId && t.OrganisationId == organisationId && !t.IsDeleted, ct);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        if (ticket.RowVersion != request.RowVersion)
            throw new ConcurrencyConflictException("Concurrency conflict. Please refresh and retry.");

        ticket.AssignedToUserId = request.AssigneeUserId;
        ticket.UpdatedAtUtc = DateTimeOffset.UtcNow;
        ticket.RowVersion += 1;

        await _db.SaveChangesAsync(ct);

        return new TicketDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedByUserId,
            ticket.AssignedToUserId,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.RowVersion);
    }

    public async Task<TicketDto> CompleteAsync(Guid ticketId, CompleteTicketRequest request, CancellationToken ct)
    {
        var organisationId = _tenant.OrganisationId;
        var userId = _tenant.UserId;
        var role = _tenant.Role;

        var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        var q = _db.Tickets
            .Where(t => t.Id == ticketId && t.OrganisationId == organisationId && !t.IsDeleted);

        if (!isAdmin)
        {
            q = q.Where(t => t.CreatedByUserId == userId || t.AssignedToUserId == userId);
        }

        var ticket = await q.SingleOrDefaultAsync(ct);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        if (ticket.RowVersion != request.RowVersion)
            throw new ConcurrencyConflictException("Concurrency conflict. Please refresh and retry.");

        if (string.Equals(ticket.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Ticket is already completed.");

        ticket.Status = "Completed";
        ticket.UpdatedAtUtc = DateTimeOffset.UtcNow;
        ticket.RowVersion += 1;

        await _db.SaveChangesAsync(ct);

        return new TicketDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedByUserId,
            ticket.AssignedToUserId,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.RowVersion);
    }

    public async Task SoftDeleteAsync(Guid ticketId, int rowVersion, CancellationToken ct)
    {
        var isAdmin = string.Equals(_tenant.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
            throw new ForbiddenException("Admin role required.");

        var organisationId = _tenant.OrganisationId;

        var ticket = await _db.Tickets
            .SingleOrDefaultAsync(t => t.Id == ticketId && t.OrganisationId == organisationId && !t.IsDeleted, ct);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        if (ticket.RowVersion != rowVersion)
            throw new ConcurrencyConflictException("Concurrency conflict. Please refresh and retry.");

        ticket.IsDeleted = true;
        ticket.UpdatedAtUtc = DateTimeOffset.UtcNow;
        ticket.RowVersion += 1;

        await _db.SaveChangesAsync(ct);
    }
}
