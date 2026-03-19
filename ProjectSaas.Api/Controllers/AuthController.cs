using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectSaas.Api.Application.Abstractions.Auth;
using ProjectSaas.Api.Contracts.Requests.Auth;
using ProjectSaas.Api.Contracts.Responses.Auth;
using ProjectSaas.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ProjectSaas.Api.Contracts.Responses.Users;

namespace ProjectSaas.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly RefreshCookieOptions _refreshCookieOptions;
    private readonly RefreshTokenOptions _refreshTokenOptions;

    public AuthController(
        IAuthService authService,
        IOptions<RefreshCookieOptions> refreshCookieOptions,
        IOptions<RefreshTokenOptions> refreshTokenOptions)
    {
        _authService = authService;
        _refreshCookieOptions = refreshCookieOptions.Value;
        _refreshTokenOptions = refreshTokenOptions.Value;
    }

    [EnableRateLimiting("auth-login")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);

        Response.Cookies.Append(
            _refreshCookieOptions.Name,
            result.RefreshToken,
            BuildRefreshCookieOptions());

        var response = new LoginResponse
        {
            AccessToken = result.AccessToken,
            User = new AuthUserDto
            {
                Id = result.UserId,
                Email = result.Email,
                Role = result.Role,
                OrganisationId = result.OrganisationId
            }
        };

        return Ok(response);
    }

    [EnableRateLimiting("auth-refresh")]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue(_refreshCookieOptions.Name, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var requestIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _authService.RefreshAsync(
            refreshToken,
            requestIpAddress,
            ct);

        Response.Cookies.Append(
            _refreshCookieOptions.Name,
            result.RefreshToken,
            BuildRefreshCookieOptions());

        return Ok(new LoginResponse
        {
            AccessToken = result.AccessToken,
            User = new AuthUserDto
            {
                Id = result.UserId,
                Email = result.Email,
                Role = result.Role,
                OrganisationId = result.OrganisationId
            }
        });
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue(_refreshCookieOptions.Name, out var refreshToken) &&
            !string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authService.LogoutAsync(refreshToken, ct);
        }

        Response.Cookies.Delete(
            _refreshCookieOptions.Name,
            BuildRefreshCookieDeletionOptions());

        return NoContent();
    }

    private CookieOptions BuildRefreshCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = _refreshCookieOptions.HttpOnly,
            Secure = _refreshCookieOptions.Secure,
            SameSite = ParseSameSite(_refreshCookieOptions.SameSite),
            Path = _refreshCookieOptions.Path,
            Expires = DateTimeOffset.UtcNow.AddDays(_refreshTokenOptions.DaysToLive)
        };
    }

    private CookieOptions BuildRefreshCookieDeletionOptions()
    {
        return new CookieOptions
        {
            HttpOnly = _refreshCookieOptions.HttpOnly,
            Secure = _refreshCookieOptions.Secure,
            SameSite = ParseSameSite(_refreshCookieOptions.SameSite),
            Path = _refreshCookieOptions.Path
        };
    }

    private static SameSiteMode ParseSameSite(string sameSite)
    {
        return sameSite.ToLowerInvariant() switch
        {
            "strict" => SameSiteMode.Strict,
            "none" => SameSiteMode.None,
            _ => SameSiteMode.Lax
        };
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var subClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue("sub");

        if (!Guid.TryParse(subClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId, ct);
        return Ok(user);
    }
}