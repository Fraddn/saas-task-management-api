using Microsoft.AspNetCore.Identity;
using ProjectSaas.Api.Application.Interfaces;

namespace ProjectSaas.Api.Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(null!, passwordHash, password);
        return result == PasswordVerificationResult.Success;
    }
}
