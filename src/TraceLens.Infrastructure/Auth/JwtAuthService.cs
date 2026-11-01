using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TraceLens.Application.Common.Interfaces;

namespace TraceLens.Infrastructure.Auth;

/// <summary>
/// Demo-only JWT auth — validates against a single config-based user.
/// Production: replace with a User entity (DB) + hashed passwords (BCrypt/Argon2).
/// </summary>
public sealed class JwtAuthService : IAuthService
{
    private readonly JwtOptions _options;

    public JwtAuthService(IOptions<JwtOptions> options) => _options = options.Value;

    public Task<AuthResult> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Task.FromResult(new AuthResult(false, null, null, null, null, null, "Username and password required."));

        var user = _options.DemoUser;
        var usernameMatches = string.Equals(username, user.Username, StringComparison.OrdinalIgnoreCase);
        // NOTE: demo-only plain comparison — production must use BCrypt/Argon2 verify
        var passwordMatches = password == user.Password;

        if (!usernameMatches || !passwordMatches)
            return Task.FromResult(new AuthResult(false, null, null, null, null, null, "Invalid username or password."));

        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);
        var token = GenerateToken(user.Username, user.Role, expiresAt);

        return Task.FromResult(new AuthResult(
            true,
            token,
            user.Username,
            user.DisplayName,
            user.Role,
            expiresAt,
            null));
    }

    private string GenerateToken(string username, string role, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
