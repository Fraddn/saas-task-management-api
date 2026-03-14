using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectSaas.Api.Application.Interfaces;
using ProjectSaas.Api.Application.Models;
using ProjectSaas.Api.Contracts.Requests.Companies;
using ProjectSaas.Api.Contracts.Responses.Companies;

namespace ProjectSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterCompanyResult>> CreateCompany(
        [FromBody] RegisterCompanyRequest request,
        CancellationToken ct)
    {
        var result = await _companyService.RegisterAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<CompanyDto>> GetCurrent(CancellationToken ct)
    {
        var result = await _companyService.GetCurrentAsync(ct);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("me")]
    public async Task<ActionResult<CompanyDto>> UpdateCurrent(
        [FromBody] UpdateCompanyRequest request,
        CancellationToken ct)
    {
        var result = await _companyService.UpdateCurrentAsync(request, ct);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteCurrent(CancellationToken ct)
    {
        await _companyService.SoftDeleteCurrentAsync(ct);
        return NoContent();
    }
}