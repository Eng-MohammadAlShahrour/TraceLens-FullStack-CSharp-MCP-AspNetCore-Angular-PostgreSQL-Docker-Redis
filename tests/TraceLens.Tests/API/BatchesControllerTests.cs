using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TraceLens.API.Controllers;
using TraceLens.Application.Features.Batches.CreateBatch;
using TraceLens.Application.Features.Batches.GetBatchById;
using TraceLens.Application.Features.Batches.GetBatchesList;

namespace TraceLens.Tests.API;

public class BatchesControllerTests
{
    private readonly ISender _mediator = Substitute.For<ISender>();

    [Fact]
    public async Task Create_Returns201WithLocation()
    {
        var response = new CreateBatchResponse(Guid.NewGuid(), "B-001", "Created", DateTime.UtcNow);
        _mediator.Send(Arg.Any<CreateBatchCommand>(), Arg.Any<CancellationToken>()).Returns(response);

        var controller = new BatchesController(_mediator);
        var cmd = new CreateBatchCommand("B-001", Guid.NewGuid(), Guid.NewGuid(), 100, "pcs");

        var result = await controller.Create(cmd, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        created.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetById_ExistingBatch_Returns200()
    {
        var id = Guid.NewGuid();
        var dto = new BatchDto(id, "B-001", "QCPassed", 100, "pcs", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, null, null);
        _mediator.Send(Arg.Any<GetBatchByIdQuery>(), Arg.Any<CancellationToken>()).Returns(dto);

        var controller = new BatchesController(_mediator);
        var result = await controller.GetById(id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_MissingBatch_Returns404()
    {
        _mediator.Send(Arg.Any<GetBatchByIdQuery>(), Arg.Any<CancellationToken>()).Returns((BatchDto?)null);

        var controller = new BatchesController(_mediator);
        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetList_Returns200WithPayload()
    {
        var response = new GetBatchesListResponse(new List<BatchListItemDto>(), 0, 1, 20);
        _mediator.Send(Arg.Any<GetBatchesListQuery>(), Arg.Any<CancellationToken>()).Returns(response);

        var controller = new BatchesController(_mediator);
        var result = await controller.GetList(1, 20, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().Be(response);
    }

    [Fact]
    public async Task Create_PassesCommandToMediator()
    {
        _mediator.Send(Arg.Any<CreateBatchCommand>(), Arg.Any<CancellationToken>())
            .Returns(new CreateBatchResponse(Guid.NewGuid(), "B-X", "Created", DateTime.UtcNow));

        var controller = new BatchesController(_mediator);
        var cmd = new CreateBatchCommand("B-X", Guid.NewGuid(), Guid.NewGuid(), 5, "kg");

        await controller.Create(cmd, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<CreateBatchCommand>(c => c.BatchNumber == "B-X" && c.ProducedQuantity == 5 && c.ProducedQuantityUnit == "kg"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetList_DefaultsToPage1Size20()
    {
        _mediator.Send(Arg.Any<GetBatchesListQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetBatchesListResponse(new List<BatchListItemDto>(), 0, 1, 20));

        var controller = new BatchesController(_mediator);
        await controller.GetList(1, 20, CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<GetBatchesListQuery>(q => q.Page == 1 && q.PageSize == 20),
            Arg.Any<CancellationToken>());
    }
}