using ProjectSaas.Api.Application.Models;
using ProjectSaas.Api.Contracts.Requests.Companies;

namespace ProjectSaas.Api.Application.Interfaces;

public interface ICompanyService
{
    Task<RegisterCompanyResult> RegisterAsync(RegisterCompanyRequest request, CancellationToken ct = default);
}
