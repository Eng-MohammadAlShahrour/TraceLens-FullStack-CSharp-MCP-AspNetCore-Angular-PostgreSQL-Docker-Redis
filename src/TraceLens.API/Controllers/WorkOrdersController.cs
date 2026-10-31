using MediatR;
using Microsoft.AspNetCore.Mvc;
using TraceLens.Application.Features.WorkOrders.CreateWorkOrder;
using TraceLens.Application.Features.WorkOrders.GetWorkOrdersList;

namespace TraceLens.API.Controllers;

[ApiController]
[Route("api/work-orders")]
public sealed class WorkOrdersController : ControllerBase
{
    private readonly ISender _mediator;

    public WorkOrdersController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(CreateWorkOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Created($"/api/work-orders/{result.Id}", result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetWorkOrdersListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWorkOrdersListQuery(page, pageSize), ct);
        return Ok(result);
    }
}
