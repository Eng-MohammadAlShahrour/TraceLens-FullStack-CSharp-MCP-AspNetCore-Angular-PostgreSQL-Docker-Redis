using FluentAssertions;
using NSubstitute;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Application.Features.Batches.CreateBatch;
using TraceLens.Domain.Enums;
using TraceLens.Tests.Helpers;

namespace TraceLens.Tests.Application;

public class CreateBatchCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesBatch()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateBatchCommandHandler(db, Substitute.For<IBatchNotifier>());

        var cmd = new CreateBatchCommand("B-001", Guid.NewGuid(), Guid.NewGuid(), 100, "pcs");
        var response = await handler.Handle(cmd, CancellationToken.None);

        response.Should().NotBeNull();
        response.BatchNumber.Should().Be("B-001");
        response.Status.Should().Be(BatchStatus.Created.ToString());

        db.Batches.Should().ContainSingle(b => b.BatchNumber.Value == "B-001");
    }

    [Fact]
    public async Task Handle_PersistsProducedQuantityAndUnit()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateBatchCommandHandler(db, Substitute.For<IBatchNotifier>());

        var cmd = new CreateBatchCommand("B-002", Guid.NewGuid(), Guid.NewGuid(), 250.5m, "kg");
        await handler.Handle(cmd, CancellationToken.None);

        var batch = db.Batches.Single();
        batch.ProducedQuantity.Value.Should().Be(250.5m);
        batch.ProducedQuantity.Unit.Should().Be("kg");
    }

    [Fact]
    public async Task Handle_ReturnsGeneratedId()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateBatchCommandHandler(db, Substitute.For<IBatchNotifier>());

        var cmd = new CreateBatchCommand("B-003", Guid.NewGuid(), Guid.NewGuid(), 10, "pcs");
        var response = await handler.Handle(cmd, CancellationToken.None);

        response.Id.Should().NotBe(Guid.Empty);
        db.Batches.Single().Id.Should().Be(response.Id);
    }

    [Fact]
    public async Task Handle_WithInvalidWorkOrderId_Throws()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateBatchCommandHandler(db, Substitute.For<IBatchNotifier>());

        var cmd = new CreateBatchCommand("B-004", Guid.Empty, Guid.NewGuid(), 1, "pcs");

        var act = async () => await handler.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("workOrderId");
    }

    [Fact]
    public async Task Handle_WithInvalidProductId_Throws()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateBatchCommandHandler(db, Substitute.For<IBatchNotifier>());

        var cmd = new CreateBatchCommand("B-005", Guid.NewGuid(), Guid.Empty, 1, "pcs");

        var act = async () => await handler.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("productId");
    }

    [Fact]
    public async Task Handle_NotifiesSignalRAfterCreate()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var notifier = Substitute.For<IBatchNotifier>();
        var handler = new CreateBatchCommandHandler(db, notifier);

        var cmd = new CreateBatchCommand("B-NOTIFY", Guid.NewGuid(), Guid.NewGuid(), 10, "pcs");
        var response = await handler.Handle(cmd, CancellationToken.None);

        await notifier.Received(1).NotifyBatchCreatedAsync(
            response.Id,
            "B-NOTIFY",
            BatchStatus.Created.ToString(),
            Arg.Any<CancellationToken>());
    }
}
