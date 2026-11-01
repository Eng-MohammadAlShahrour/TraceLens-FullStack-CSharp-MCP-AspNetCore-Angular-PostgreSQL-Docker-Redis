using MediatR;

namespace TraceLens.Application.Features.Auth.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<LoginResponse?>;

public sealed record LoginResponse(
    string Token,
    string Username,
    string DisplayName,
    string Role,
    DateTime ExpiresAt);
