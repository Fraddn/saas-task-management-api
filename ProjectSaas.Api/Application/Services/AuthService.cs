using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Application.Abstractions.Auth;
using ProjectSaas.Api.Application.Auth;
using ProjectSaas.Api.Application.Exceptions;
using ProjectSaas.Api.Contracts.Requests.Auth;
using ProjectSaas.Api.Infrastructure.Data;
using ProjectSaas.Api.Application.Interfaces;
using ProjectSaas.Api.Application.Abstractions.Security;

namespace ProjectSaas.Api.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokens;

    public AuthService(
        AppDbContext db,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenService refreshTokens)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokens = refreshTokens;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var org = await _db.Organisations
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Slug == request.CompanySlug)
            .Select(o => new { o.Id, o.Slug })
            .SingleOrDefaultAsync(ct);

        if (org is null)
            throw new InvalidCredentialsException();

        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.OrganisationId == org.Id && u.Email == request.Email)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.PasswordHash,
                u.Role,
                u.IsDisabled,
                u.OrganisationId,
                u.IsPlatformAdmin
            })
            .SingleOrDefaultAsync(ct);

        if (user is null)
            throw new InvalidCredentialsException();

        var ok = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!ok)
            throw new InvalidCredentialsException();

        if (user.IsDisabled)
            throw new ForbiddenException("User account is disabled.");

        var accessToken = _tokenService.CreateAccessToken(
            new TokenUser(
                user.Id,
                user.OrganisationId,
                user.Email,
                user.Role,
                user.IsPlatformAdmin
            ));

        var familyId = Guid.NewGuid();

        var refreshToken = await _refreshTokens.IssueAsync(
            user.OrganisationId,
            user.Id,
            familyId,
            ct);

        return new LoginResult(
            accessToken,
            refreshToken,
            user.Id,
            user.Email,
            user.Role,
            user.OrganisationId
        );
    }

    public async Task<LoginResult> RefreshAsync(
        string refreshToken,
        string? requestIpAddress,
        CancellationToken ct)
    {
        var rotated = await _refreshTokens.RotateAsync(
            refreshToken,
            requestIpAddress,
            ct);

        if (rotated is null)
            throw new InvalidRefreshTokenException();

        var user = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == rotated.UserId && u.OrganisationId == rotated.OrganisationId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Role,
                u.IsDisabled,
                u.OrganisationId,
                u.IsPlatformAdmin
            })
            .SingleOrDefaultAsync(ct);

        if (user is null)
            throw new InvalidCredentialsException();

        if (user.IsDisabled)
            throw new ForbiddenException("User account is disabled.");

        var accessToken = _tokenService.CreateAccessToken(
            new TokenUser(
                user.Id,
                user.OrganisationId,
                user.Email,
                user.Role,
                user.IsPlatformAdmin
            ));

        return new LoginResult(
            accessToken,
            rotated.RefreshToken,
            user.Id,
            user.Email,
            user.Role,
            user.OrganisationId
        );
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        await _refreshTokens.RevokeAsync(refreshToken, ct);
    }
}
