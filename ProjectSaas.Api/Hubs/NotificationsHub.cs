using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjectSaas.Api.Realtime;

namespace ProjectSaas.Api.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub
{
  public override async Task OnConnectedAsync()
  {
    var organisationIdClaim = Context.User?.FindFirst("orgId")?.Value;
    var userIdClaim = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    if (!Guid.TryParse(organisationIdClaim, out var organisationId))
    {
      throw new HubException("Missing or invalid orgId claim.");
    }

    if (!Guid.TryParse(userIdClaim, out var userId))
    {
      throw new HubException("Missing or invalid sub claim.");
    }

    var groupName = NotificationGroupNameBuilder.ForRecipient(organisationId, userId);

    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    await base.OnConnectedAsync();
  }
}