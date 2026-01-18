using Microsoft.AspNetCore.Mvc;
using ProjectSaas.Api.Application.Abstractions.Auth;
using ProjectSaas.Api.Contracts.Requests.Auth;
using ProjectSaas.Api.Contracts.Responses.Auth;

namespace ProjectSaas.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

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
}
