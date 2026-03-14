using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectSaas.Api.Application.Tickets;

namespace ProjectSaas.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;

    public TicketsController(ITicketService tickets)
    {
        _tickets = tickets;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> GetList(
        [FromQuery] TicketListQuery query,
        CancellationToken ct)
    {
        var result = await _tickets.GetListAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<ActionResult<TicketDto>> GetById(Guid ticketId, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId, ct);
        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken ct)
    {
        var ticket = await _tickets.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { ticketId = ticket.Id }, ticket);
    }

    [HttpPatch("{ticketId:guid}")]
    public async Task<ActionResult<TicketDto>> Update(
        Guid ticketId,
        [FromBody] UpdateTicketRequest request,
        CancellationToken ct)
    {
        var ticket = await _tickets.UpdateAsync(ticketId, request, ct);
        return Ok(ticket);
    }

    [HttpPost("{ticketId:guid}/assign")]
    public async Task<ActionResult<TicketDto>> Assign(
        Guid ticketId,
        [FromBody] AssignTicketRequest request,
        CancellationToken ct)
    {
        var ticket = await _tickets.AssignAsync(ticketId, request, ct);
        return Ok(ticket);
    }

    [HttpPost("{ticketId:guid}/complete")]
    public async Task<ActionResult<TicketDto>> Complete(
        Guid ticketId,
        [FromBody] CompleteTicketRequest request,
        CancellationToken ct)
    {
        var ticket = await _tickets.CompleteAsync(ticketId, request, ct);
        return Ok(ticket);
    }

    [HttpDelete("{ticketId:guid}")]
    public async Task<IActionResult> Delete(
        Guid ticketId,
        [FromQuery] int rowVersion,
        CancellationToken ct)
    {
        await _tickets.SoftDeleteAsync(ticketId, rowVersion, ct);
        return NoContent();
    }
}
