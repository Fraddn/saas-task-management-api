using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSaas.Api.Application.Tickets;

public interface ITicketService
{
    Task<IReadOnlyList<TicketDto>> GetListAsync(TicketListQuery query, CancellationToken ct);
    Task<TicketDto> GetByIdAsync(Guid ticketId, CancellationToken ct);
    Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken ct);
    Task<TicketDto> UpdateAsync(Guid ticketId, UpdateTicketRequest request, CancellationToken ct);
    Task<TicketDto> AssignAsync(Guid ticketId, AssignTicketRequest request, CancellationToken ct);
    Task<TicketDto> CompleteAsync(Guid ticketId, CompleteTicketRequest request, CancellationToken ct);
    Task SoftDeleteAsync(Guid ticketId, int rowVersion, CancellationToken ct);
}
