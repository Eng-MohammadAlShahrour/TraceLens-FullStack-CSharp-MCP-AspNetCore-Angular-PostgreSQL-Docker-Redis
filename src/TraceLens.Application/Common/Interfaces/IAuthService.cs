namespace TraceLens.Application.Common.Interfaces;

/// <summary>
/// Authentication service contract. Implementation lives in Infrastructure layer.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}

public sealed record AuthResult(
    bool Success,
    string? Token,
    string? Username,
    string? DisplayName,
    string? Role,
    DateTime? ExpiresAt,
    string? Error);
