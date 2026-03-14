using System.Text;

namespace ProjectSaas.Api.Infrastructure.Auth;

public interface IRefreshTokenHasher
{
    string Hash(string token);
}

public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    private readonly string _pepper;

    public RefreshTokenHasher(IConfiguration config)
    {
        _pepper = config["RefreshTokens:Pepper"]
            ?? throw new InvalidOperationException("RefreshTokens:Pepper not configured");
    }

    public string Hash(string token)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token + _pepper);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash); 
    }
}