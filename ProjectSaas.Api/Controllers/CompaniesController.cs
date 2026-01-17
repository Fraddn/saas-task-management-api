using Microsoft.AspNetCore.Mvc;
using ProjectSaas.Api.Application.Interfaces;
using ProjectSaas.Api.Application.Models;
using ProjectSaas.Api.Contracts.Requests.Companies;

namespace ProjectSaas.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPost]
    public async Task<ActionResult<RegisterCompanyResult>> CreateCompany(
    [FromBody] RegisterCompanyRequest request,
    CancellationToken ct)
    {
        var result = await _companyService.RegisterAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
