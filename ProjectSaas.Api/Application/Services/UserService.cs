using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Application.Abstractions.Security;
using ProjectSaas.Api.Application.Abstractions.Tenancy;
using ProjectSaas.Api.Application.Abstractions.Users;
using ProjectSaas.Api.Contracts.Requests.Users;
using ProjectSaas.Api.Contracts.Responses.Users;
using ProjectSaas.Api.Domain.Entities;
using ProjectSaas.Api.Infrastructure.Data;
using ProjectSaas.Api.Application.Interfaces;
using ProjectSaas.Api.Application.Exceptions;
using ProjectSaas.Api.Contracts.Common;

namespace ProjectSaas.Api.Application.Services.Users;

public sealed class UserService : IUserService
{
  private static readonly string[] AllowedRoles =
  [
      "Admin",
      "Employee"
  ];

  private readonly AppDbContext _dbContext;
  private readonly ITenantContext _tenantContext;
  private readonly IPasswordHasher _passwordHasher;

  public UserService(
      AppDbContext dbContext,
      ITenantContext tenantContext,
      IPasswordHasher passwordHasher)
  {
    _dbContext = dbContext;
    _tenantContext = tenantContext;
    _passwordHasher = passwordHasher;
  }

  public async Task<PagedResult<UserDto>> GetListAsync(int page, int pageSize, CancellationToken ct)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 20 : pageSize;
    pageSize = pageSize > 100 ? 100 : pageSize;

    IQueryable<User> query = _dbContext.Users.AsNoTracking();

    if (!_tenantContext.IsPlatformAdmin)
    {
      var organisationId = GetOrganisationId();
      query = query.Where(u => u.OrganisationId == organisationId);
    }

    query = query.OrderBy(u => u.Email);

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new UserDto
        {
          Id = u.Id,
          Email = u.Email,
          FirstName = u.FirstName,
          LastName = u.LastName,
          Role = u.Role,
          IsDisabled = u.IsDisabled,
          OrganisationId = u.OrganisationId,
          CreatedAtUtc = u.CreatedAtUtc,
          UpdatedAtUtc = u.UpdatedAtUtc
        })
        .ToListAsync(ct);

    return new PagedResult<UserDto>
    {
      Items = items,
      Page = page,
      PageSize = pageSize,
      TotalCount = totalCount,
      TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
    };
  }

  public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct)
  {
    var organisationId = GetOrganisationId();

    ValidateCreateRequest(request);

    var normalizedEmail = request.Email.Trim().ToLowerInvariant();
    var normalizedRole = NormalizeRole(request.Role);

    var emailExists = await _dbContext.Users.AnyAsync(
        u => u.OrganisationId == organisationId &&
             u.Email == normalizedEmail,
        ct);

    if (emailExists)
    {
      throw new InvalidOperationException("A user with this email already exists in this organisation.");
    }

    var user = new User
    {
      Id = Guid.NewGuid(),
      OrganisationId = organisationId,
      Email = normalizedEmail,
      FirstName = request.FirstName.Trim(),
      LastName = request.LastName.Trim(),
      PasswordHash = _passwordHasher.Hash(request.Password),
      Role = normalizedRole,
      IsPlatformAdmin = false,
      IsDisabled = false,
      CreatedAtUtc = DateTime.UtcNow,
      UpdatedAtUtc = DateTime.UtcNow
    };

    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync(ct);

    return new UserDto
    {
      Id = user.Id,
      Email = user.Email,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Role = user.Role,
      IsDisabled = user.IsDisabled,
      OrganisationId = user.OrganisationId,
      CreatedAtUtc = user.CreatedAtUtc,
      UpdatedAtUtc = user.UpdatedAtUtc
    };
  }

  public async Task<UserDto> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct)
  {
    ValidateUpdateRequest(request);

    User? user;

    if (_tenantContext.IsPlatformAdmin)
    {
      user = await _dbContext.Users.FirstOrDefaultAsync(
          u => u.Id == userId,
          ct);
    }
    else
    {
      var organisationId = GetOrganisationId();

      user = await _dbContext.Users.FirstOrDefaultAsync(
          u => u.Id == userId && u.OrganisationId == organisationId,
          ct);
    }

    if (user is null)
    {
      throw new KeyNotFoundException("User not found.");
    }

    if (request.Email is not null)
    {
      var normalizedEmail = request.Email.Trim().ToLowerInvariant();

      if (string.IsNullOrWhiteSpace(normalizedEmail))
      {
        throw new ArgumentException("Email is required.");
      }

      var duplicateEmailExists = await _dbContext.Users.AnyAsync(
          u => u.OrganisationId == user.OrganisationId &&
               u.Id != userId &&
               u.Email == normalizedEmail,
          ct);

      if (duplicateEmailExists)
      {
        throw new InvalidOperationException("A user with this email already exists in this organisation.");
      }

      user.Email = normalizedEmail;
    }

    if (request.Role is not null)
    {
      var newRole = NormalizeRole(request.Role);
      var currentRole = user.Role;

      var isCurrentAdmin = string.Equals(currentRole, "Admin", StringComparison.OrdinalIgnoreCase);
      var isRequestedAdmin = string.Equals(newRole, "Admin", StringComparison.OrdinalIgnoreCase);
      var roleIsActuallyChanging = !string.Equals(currentRole, newRole, StringComparison.OrdinalIgnoreCase);

      if (roleIsActuallyChanging && isCurrentAdmin && !isRequestedAdmin)
      {
        var activeAdminCount = await _dbContext.Users.CountAsync(
            u => u.OrganisationId == user.OrganisationId &&
                 u.Role == "Admin" &&
                 u.IsDisabled == false,
            ct);

        if (activeAdminCount <= 1)
        {
          throw new InvalidOperationException("Cannot demote the last active admin in the organisation.");
        }
      }

      user.Role = newRole;
    }

    if (request.IsDisabled.HasValue)
    {
      var requestedIsDisabled = request.IsDisabled.Value;
      var isCurrentlyAdmin = string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase);
      var isActuallyBecomingDisabled = requestedIsDisabled && user.IsDisabled == false;

      if (requestedIsDisabled && user.Id == _tenantContext.UserId)
      {
        throw new ArgumentException("You cannot disable your own account.");
      }

      if (isActuallyBecomingDisabled && isCurrentlyAdmin)
      {
        var activeAdminCount = await _dbContext.Users.CountAsync(
            u => u.OrganisationId == user.OrganisationId &&
                 u.Role == "Admin" &&
                 u.IsDisabled == false,
            ct);

        if (activeAdminCount <= 1)
        {
          throw new InvalidOperationException("Cannot disable the last active admin in the organisation.");
        }
      }

      user.IsDisabled = requestedIsDisabled;
    }

    user.UpdatedAtUtc = DateTime.UtcNow;

    await _dbContext.SaveChangesAsync(ct);

    return new UserDto
    {
      Id = user.Id,
      Email = user.Email,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Role = user.Role,
      IsDisabled = user.IsDisabled,
      OrganisationId = user.OrganisationId,
      CreatedAtUtc = user.CreatedAtUtc,
      UpdatedAtUtc = user.UpdatedAtUtc
    };
  }

  public async Task DeleteAsync(Guid userId, CancellationToken ct)
  {
    var currentUserId = _tenantContext.UserId;

    User? user;

    if (_tenantContext.IsPlatformAdmin)
    {
      user = await _dbContext.Users.FirstOrDefaultAsync(
          u => u.Id == userId,
          ct);
    }
    else
    {
      var organisationId = _tenantContext.OrganisationId;

      user = await _dbContext.Users.FirstOrDefaultAsync(
          u => u.Id == userId && u.OrganisationId == organisationId,
          ct);
    }

    if (user is null)
    {
      throw new NotFoundException("User not found.");
    }

    if (user.Id == currentUserId)
    {
      throw new ArgumentException("You cannot disable your own account.");
    }

    if (user.IsDisabled)
    {
      return;
    }

    if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
    {
      var activeAdminCount = await _dbContext.Users.CountAsync(
          u => u.OrganisationId == user.OrganisationId &&
               u.Role == "Admin" &&
               !u.IsDisabled,
          ct);

      if (activeAdminCount <= 1)
      {
        throw new InvalidOperationException("Cannot disable the last active admin in the organisation.");
      }
    }

    user.IsDisabled = true;
    user.UpdatedAtUtc = DateTime.UtcNow;

    await _dbContext.SaveChangesAsync(ct);
  }

  public async Task<UserDto> GetByIdAsync(Guid userId, CancellationToken ct)
  {
    IQueryable<User> query = _dbContext.Users.AsNoTracking();

    if (_tenantContext.IsPlatformAdmin)
    {
      query = query.Where(u => u.Id == userId);
    }
    else
    {
      var organisationId = _tenantContext.OrganisationId;
      query = query.Where(u => u.Id == userId && u.OrganisationId == organisationId);
    }

    var user = await query
        .Select(u => new UserDto
        {
          Id = u.Id,
          Email = u.Email,
          FirstName = u.FirstName,
          LastName = u.LastName,
          Role = u.Role,
          IsDisabled = u.IsDisabled,
          OrganisationId = u.OrganisationId,
          CreatedAtUtc = u.CreatedAtUtc,
          UpdatedAtUtc = u.UpdatedAtUtc
        })
        .SingleOrDefaultAsync(ct);

    if (user is null)
    {
      throw new NotFoundException("User not found.");
    }

    return user;
  }

  private Guid GetOrganisationId()
  {
    return _tenantContext.OrganisationId;
  }

  private static void ValidateCreateRequest(CreateUserRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.Email))
    {
      throw new ArgumentException("Email is required.");
    }

    if (string.IsNullOrWhiteSpace(request.FirstName))
    {
      throw new ArgumentException("First name is required.");
    }

    if (string.IsNullOrWhiteSpace(request.LastName))
    {
      throw new ArgumentException("Last name is required.");
    }

    if (string.IsNullOrWhiteSpace(request.Password))
    {
      throw new ArgumentException("Password is required.");
    }

    if (string.IsNullOrWhiteSpace(request.Role))
    {
      throw new ArgumentException("Role is required.");
    }
  }

  private static void ValidateUpdateRequest(UpdateUserRequest request)
  {
    var hasAnyField =
        request.Email is not null ||
        request.Role is not null ||
        request.IsDisabled.HasValue;

    if (!hasAnyField)
    {
      throw new ArgumentException("At least one field must be provided.");
    }
  }

  private static string NormalizeRole(string role)
  {
    var trimmedRole = role.Trim();

    var matchedRole = AllowedRoles.FirstOrDefault(r =>
        string.Equals(r, trimmedRole, StringComparison.OrdinalIgnoreCase));

    if (matchedRole is null)
    {
      throw new ArgumentException("Invalid role. Allowed values are Admin and Employee.");
    }

    return matchedRole;
  }
}
