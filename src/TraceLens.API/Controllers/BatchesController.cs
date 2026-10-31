using MediatR;
using Microsoft.AspNetCore.Mvc;
using TraceLens.Application.Features.Batches.CreateBatch;
using TraceLens.Application.Features.Batches.FailQC;
using TraceLens.Application.Features.Batches.GetBatchById;
using TraceLens.Application.Features.Batches.GetBatchesList;
using TraceLens.Application.Features.Batches.PassQC;
using TraceLens.Application.Features.Batches.QuarantineBatch;
using TraceLens.Application.Features.Batches.ShipBatch;
using TraceLens.Application.Features.Batches.StartProduction;

namespace TraceLens.API.Controllers;

[ApiController]
[Route("api/batches")]
public sealed class BatchesController : ControllerBase
{
    private readonly ISender _mediator;

    public BatchesController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(CreateBatchResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBatchCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBatchByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetBatchesListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetBatchesListQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start-production")]
    public async Task<IActionResult> StartProduction(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new StartProductionCommand(id), ct));

    [HttpPost("{id:guid}/pass-qc")]
    public async Task<IActionResult> PassQC(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new PassQCCommand(id), ct));

    [HttpPost("{id:guid}/fail-qc")]
    public async Task<IActionResult> FailQC(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new FailQCCommand(id), ct));

    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> Ship(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new ShipBatchCommand(id), ct));

    [HttpPost("{id:guid}/quarantine")]
    public async Task<IActionResult> Quarantine(Guid id, [FromBody] QuarantineRequest body, CancellationToken ct)
        => Ok(await _mediator.Send(new QuarantineBatchCommand(id, body.Reason), ct));
}

public sealed record QuarantineRequest(string Reason);
