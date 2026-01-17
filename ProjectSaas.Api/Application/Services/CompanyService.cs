using ProjectSaas.Api.Application.Interfaces;
using ProjectSaas.Api.Application.Models;
using ProjectSaas.Api.Contracts.Requests.Companies;
using ProjectSaas.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ProjectSaas.Api.Domain.Entities;

namespace ProjectSaas.Api.Application.Services;

public sealed class CompanyService : ICompanyService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public CompanyService(AppDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterCompanyResult> RegisterAsync(RegisterCompanyRequest request, CancellationToken ct = default)
    {
        var name = request.CompanyName?.Trim() ?? "";
        var slug = request.CompanySlug?.Trim().ToLowerInvariant() ?? "";
        var email = request.AdminEmail?.Trim().ToLowerInvariant() ?? "";
        var password = request.AdminPassword ?? "";

        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("CompanyName is required.");
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("CompanySlug is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("AdminEmail is required.");
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

}