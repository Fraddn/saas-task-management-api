using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Application.Abstractions.Auth;
using ProjectSaas.Api.Application.Abstractions.Security;
using ProjectSaas.Api.Application.Auth;
using ProjectSaas.Api.Application.Exceptions;
using ProjectSaas.Api.Contracts.Requests.Auth;
using ProjectSaas.Api.Infrastructure.Data;
using ProjectSaas.Api.Application.Interfaces;

namespace ProjectSaas.Api.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext db, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        // 1) Resolve tenant by slug
        var org = await _db.Organisations
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Slug == request.CompanySlug)
            .Select(o => new { o.Id, o.Slug })
            .SingleOrDefaultAsync(ct);

        if (org is null)
            throw new InvalidCredentialsException(); // do not reveal whether tenant exists

        // 2) Find user by (orgId + email)
        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.OrganisationId == org.Id && u.Email == request.Email)
            .Select(u => new { u.Id, u.Email, u.PasswordHash, u.Role, u.IsDisabled, u.OrganisationId })
            .SingleOrDefaultAsync(ct);

        if (user is null)
            throw new InvalidCredentialsException();

        // 3) Verify password
        var ok = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!ok)
            throw new InvalidCredentialsException();

        // 4) Disabled check
        if (user.IsDisabled)
            throw new ForbiddenException("User account is disabled.");

        // 5) Issue JWT
        var token = _tokenService.CreateAccessToken(new TokenUser(
            user.Id,
            user.OrganisationId,
            user.Role,
            user.Email
        ));

        return new LoginResult(
            token,
            user.Id,
            user.Email,
            user.Role,
            user.OrganisationId
        );
    }
}
