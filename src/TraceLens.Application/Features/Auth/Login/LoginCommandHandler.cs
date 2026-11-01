using MediatR;
using TraceLens.Application.Common.Interfaces;

namespace TraceLens.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse?>
{
    private readonly IAuthService _auth;

    public LoginCommandHandler(IAuthService auth) => _auth = auth;

    public async Task<LoginResponse?> Handle(LoginCommand request, CancellationToken ct)
    {
        var result = await _auth.AuthenticateAsync(request.Username, request.Password, ct);

        if (!result.Success || result.Token is null || result.ExpiresAt is null)
            return null;

        return new LoginResponse(
            result.Token,
            result.Username ?? request.Username,
            result.DisplayName ?? string.Empty,
            result.Role ?? "Admin",
            result.ExpiresAt.Value);
    }
}
