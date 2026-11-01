using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using TraceLens.Application.Features.Activity.GetActivity;

namespace TraceLens.API.Controllers;

[ApiController]
[Route("api/activity")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public sealed class ActivityController : ControllerBase
{
    private readonly ISender _mediator;
    public ActivityController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(GetActivityResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent(
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetActivityQuery(limit), ct));
}
