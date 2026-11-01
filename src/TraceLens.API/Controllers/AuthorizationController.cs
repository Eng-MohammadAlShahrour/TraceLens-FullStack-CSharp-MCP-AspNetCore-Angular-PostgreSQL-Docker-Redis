using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using TraceLens.Application.Common.Interfaces;

namespace TraceLens.API.Controllers;

[ApiController]
public sealed class AuthorizationController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthorizationController(IAuthService auth) => _auth = auth;

    [HttpPost("~/connect/token")]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenID Connect request could not be retrieved.");

        // ── Password grant ──
        if (request.IsPasswordGrantType())
        {
            var result = await _auth.AuthenticateAsync(
                request.Username ?? string.Empty,
                request.Password ?? string.Empty);

            if (!result.Success)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid username or password."
                    }));
            }

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: ClaimsIdentity.DefaultNameClaimType,
                roleType: ClaimsIdentity.DefaultRoleClaimType);

            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, result.Username!));
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, result.DisplayName ?? result.Username!));
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, result.Role ?? "Admin"));

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes("api");
            principal.SetDestinations(_ => new[] { OpenIddictConstants.Destinations.AccessToken });

            // ── Attach custom response params for Angular UI ──
            var properties = new AuthenticationProperties();
            properties.SetParameter("username", result.Username!);
            properties.SetParameter("display_name", result.DisplayName ?? result.Username!);
            properties.SetParameter("role", result.Role ?? "Admin");

            return SignIn(principal, properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // ── Refresh token grant ──
        if (request.IsRefreshTokenGrantType())
        {
            var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!info.Succeeded)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return SignIn(info.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }
}
