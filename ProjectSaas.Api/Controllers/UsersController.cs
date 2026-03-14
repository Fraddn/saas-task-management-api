using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectSaas.Api.Application.Abstractions.Users;
using ProjectSaas.Api.Contracts.Requests.Users;
using ProjectSaas.Api.Contracts.Responses.Users;
using ProjectSaas.Api.Contracts.Common;

namespace ProjectSaas.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdminOnly")]
public sealed class UsersController : ControllerBase
{
  private readonly IUserService _userService;

  public UsersController(IUserService userService)
  {
    _userService = userService;
  }

  [HttpGet]
  public async Task<ActionResult<PagedResult<UserDto>>> GetList(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20,
      CancellationToken ct = default)
  {
    var users = await _userService.GetListAsync(page, pageSize, ct);
    return Ok(users);
  }

  [HttpPost]
  public async Task<ActionResult<UserDto>> Create(
      [FromBody] CreateUserRequest request,
      CancellationToken ct)
  {
    var user = await _userService.CreateAsync(request, ct);
    return CreatedAtAction(nameof(GetList), new { id = user.Id }, user);
  }

  [HttpPatch("{userId:guid}")]
  public async Task<ActionResult<UserDto>> Update(
      Guid userId,
      [FromBody] UpdateUserRequest request,
      CancellationToken ct)
  {
    var user = await _userService.UpdateAsync(userId, request, ct);
    return Ok(user);
  }

  [HttpGet("{userId:guid}")]
  public async Task<ActionResult<UserDto>> GetById(Guid userId, CancellationToken ct)
  {
    var user = await _userService.GetByIdAsync(userId, ct);
    return Ok(user);
  }


  [HttpDelete("{userId:guid}")]
  public async Task<IActionResult> Delete(
      Guid userId,
      CancellationToken ct)
  {
    await _userService.DeleteAsync(userId, ct);
    return NoContent();
  }
}