using ProjectSaas.Api.Contracts.Common;
using ProjectSaas.Api.Contracts.Requests.Users;
using ProjectSaas.Api.Contracts.Responses.Users;

namespace ProjectSaas.Api.Application.Abstractions.Users;

public interface IUserService
{
  Task<PagedResult<UserDto>> GetListAsync(int page, int pageSize, CancellationToken ct);
  Task<UserDto> GetByIdAsync(Guid userId, CancellationToken ct);
  Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct);
  Task<UserDto> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct);
  Task DeleteAsync(Guid userId, CancellationToken ct);
}
