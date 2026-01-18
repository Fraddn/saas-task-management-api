using System.Security.Claims;
using ProjectSaas.Api.Infrastructure.Tenancy;

namespace ProjectSaas.Api.Common.Middleware;

public sealed class TenantContextMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Only populate when authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? context.User.FindFirstValue("sub");

            var orgId = context.User.FindFirstValue("orgId");
            var role = context.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (Guid.TryParse(sub, out var userId) && Guid.TryParse(orgId, out var organisationId))
            {
                var accessor = context.RequestServices.GetRequiredService<TenantContextAccessor>();
                accessor.Set(organisationId, userId, role);
            }
        }

        await next(context);
    }
}
