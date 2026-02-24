using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectSaas.Api.Application.Tickets;

namespace ProjectSaas.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize] 
public sealed class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;

    public TicketsController(ITicketService tickets)
    {
        _tickets = tickets;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] TicketListQuery query, CancellationToken ct)
    {
        var result = await _tickets.GetListAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid ticketId, CancellationToken ct)
    {
        var result = await _tickets.GetByIdAsync(ticketId, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request, CancellationToken ct)
    {
        var result = await _tickets.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { ticketId = result.Id }, result);
    }

    [HttpPatch("{ticketId:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid ticketId,
        [FromBody] UpdateTicketRequest request,
        CancellationToken ct)
    {
        var result = await _tickets.UpdateAsync(ticketId, request, ct);
        return Ok(result);
    }

    // Admin-only endpoints

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{ticketId:guid}/assign")]
    public async Task<IActionResult> Assign(
        [FromRoute] Guid ticketId,
        [FromBody] AssignTicketRequest request,
        CancellationToken ct)
    {
        var result = await _tickets.AssignAsync(ticketId, request, ct);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{ticketId:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid ticketId,
        [FromQuery] int rowVersion,
        CancellationToken ct)
    {
        await _tickets.SoftDeleteAsync(ticketId, rowVersion, ct);
        return NoContent();
    }
}
