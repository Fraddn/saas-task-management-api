using ProjectSaas.Api.Application.Models;
using ProjectSaas.Api.Contracts.Requests.Companies;
using ProjectSaas.Api.Contracts.Responses.Companies;
namespace ProjectSaas.Api.Application.Interfaces;

public interface ICompanyService
{
    Task<RegisterCompanyResult> RegisterAsync(RegisterCompanyRequest request, CancellationToken ct = default);
    Task<CompanyDto> GetCurrentAsync(CancellationToken ct);
    Task<CompanyDto> UpdateCurrentAsync(UpdateCompanyRequest request, CancellationToken ct);
    Task SoftDeleteCurrentAsync(CancellationToken ct);
}
