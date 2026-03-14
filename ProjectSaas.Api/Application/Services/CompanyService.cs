using ProjectSaas.Api.Application.Interfaces;
using ProjectSaas.Api.Application.Models;
using ProjectSaas.Api.Contracts.Requests.Companies;
using ProjectSaas.Api.Contracts.Responses.Companies;
using ProjectSaas.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Domain.Entities;
using ProjectSaas.Api.Application.Abstractions.Security;
using ProjectSaas.Api.Application.Abstractions.Tenancy;

namespace ProjectSaas.Api.Application.Services;

public sealed class CompanyService : ICompanyService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;

    public CompanyService(
        AppDbContext db,
        IPasswordHasher passwordHasher,
        ITenantContext tenantContext)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
    }

    public async Task<RegisterCompanyResult> RegisterAsync(RegisterCompanyRequest request, CancellationToken ct = default)
    {
        var name = request.CompanyName?.Trim() ?? "";
        var slug = request.CompanySlug?.Trim().ToLowerInvariant() ?? "";
        var email = request.AdminEmail?.Trim().ToLowerInvariant() ?? "";
        var password = request.AdminPassword ?? "";
        var firstName = request.AdminFirstName?.Trim() ?? "";
        var lastName = request.AdminLastName?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("CompanyName is required.");
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("CompanySlug is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("AdminEmail is required.");
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("AdminFirstName is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("AdminLastName is required.");
        if (password.Length < 8) throw new ArgumentException("Password must be at least 8 characters.");

        var slugExists = await _db.Organisations.AnyAsync(o => o.Slug == slug, ct);
        if (slugExists) throw new InvalidOperationException("Company slug already exists.");

        var emailExists = await _db.Users.AnyAsync(u => u.Email == email, ct);
        if (emailExists) throw new InvalidOperationException("Admin email already exists.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var organisation = new Organisation
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.Organisations.Add(organisation);

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = _passwordHasher.Hash(password),
            Role = "Admin",
            IsDisabled = false,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.Users.Add(adminUser);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new RegisterCompanyResult
        {
            OrganisationId = organisation.Id,
            AdminUserId = adminUser.Id
        };
    }

    public async Task<CompanyDto> GetCurrentAsync(CancellationToken ct)
    {
        var organisationId = _tenantContext.OrganisationId;

        var organisation = await _db.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organisationId, ct);

        if (organisation is null)
        {
            throw new KeyNotFoundException("Company not found.");
        }

        if (organisation.IsDeleted)
        {
            throw new InvalidOperationException("Company is deleted.");
        }
        return MapToDto(organisation);
    }

    public async Task<CompanyDto> UpdateCurrentAsync(UpdateCompanyRequest request, CancellationToken ct)
    {
        var organisationId = _tenantContext.OrganisationId;

        var organisation = await _db.Organisations
            .FirstOrDefaultAsync(o => o.Id == organisationId, ct);

        if (organisation is null)
        {
            throw new KeyNotFoundException("Company not found.");
        }

        if (request.Name is not null)
        {
            var name = request.Name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be empty.");
            }

            organisation.Name = name;
        }

        if (request.Slug is not null)
        {
            var slug = request.Slug.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new ArgumentException("Slug cannot be empty.");
            }

            var slugExists = await _db.Organisations
                .AnyAsync(o => o.Id != organisation.Id && o.Slug == slug, ct);

            if (slugExists)
            {
                throw new InvalidOperationException("Company slug already exists.");
            }

            organisation.Slug = slug;
        }

        if (organisation.IsDeleted)
        {
            throw new InvalidOperationException("Company is deleted.");
        }

        organisation.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return MapToDto(organisation);
    }

    public async Task SoftDeleteCurrentAsync(CancellationToken ct)
    {
        var organisationId = _tenantContext.OrganisationId;

        var organisation = await _db.Organisations
            .FirstOrDefaultAsync(o => o.Id == organisationId, ct);

        if (organisation is null)
        {
            throw new KeyNotFoundException("Company not found.");
        }

        if (organisation.IsDeleted)
        {
            throw new InvalidOperationException("Company is already deleted.");
        }

        if (organisation.IsDeleted)
        {
            throw new InvalidOperationException("Company is deleted.");
        }

        organisation.IsDeleted = true;
        organisation.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    private static CompanyDto MapToDto(Organisation organisation)
    {
        return new CompanyDto
        {
            Id = organisation.Id,
            Name = organisation.Name,
            Slug = organisation.Slug,
            IsDeleted = organisation.IsDeleted,
            CreatedAtUtc = organisation.CreatedAtUtc,
            UpdatedAtUtc = organisation.UpdatedAtUtc
        };
    }
}