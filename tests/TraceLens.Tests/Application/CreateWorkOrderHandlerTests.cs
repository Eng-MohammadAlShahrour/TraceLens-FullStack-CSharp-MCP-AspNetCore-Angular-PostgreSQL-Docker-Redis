using FluentAssertions;
using TraceLens.Application.Common.Exceptions;
using TraceLens.Application.Features.WorkOrders.CreateWorkOrder;
using TraceLens.Tests.Helpers;

namespace TraceLens.Tests.Application;

public class CreateWorkOrderHandlerTests
{
    private static readonly Guid ValidProductId = Guid.NewGuid();
    private static readonly DateTime FutureDueDate = DateTime.UtcNow.Date.AddDays(30);

    [Fact]
    public async Task Handle_WithValidCommand_CreatesWorkOrderAndReturnsResponse()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateWorkOrderCommandHandler(db);

        var cmd = new CreateWorkOrderCommand("WO-2026-001", ValidProductId, 100, FutureDueDate);
        var response = await handler.Handle(cmd, CancellationToken.None);

        response.Should().NotBeNull();
        response.Id.Should().NotBe(Guid.Empty);
        response.OrderNumber.Should().Be("WO-2026-001");
        response.PlannedQuantity.Should().Be(100);
        response.IsClosed.Should().BeFalse();
        db.WorkOrders.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_NormalizesOrderNumber_TrimAndUpperCase()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateWorkOrderCommandHandler(db);

        var cmd = new CreateWorkOrderCommand("  wo-2026-007  ", ValidProductId, 10, FutureDueDate);
        var response = await handler.Handle(cmd, CancellationToken.None);

        response.OrderNumber.Should().Be("WO-2026-007");
        db.WorkOrders.Single().OrderNumber.Should().Be("WO-2026-007");
    }

    [Fact]
    public async Task Handle_WithDuplicateOrderNumber_ThrowsDuplicateWorkOrderException()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateWorkOrderCommandHandler(db);

        var cmd1 = new CreateWorkOrderCommand("WO-DUP", ValidProductId, 10, FutureDueDate);
        await handler.Handle(cmd1, CancellationToken.None);

        var cmd2 = new CreateWorkOrderCommand("WO-DUP", ValidProductId, 20, FutureDueDate);

        var act = async () => await handler.Handle(cmd2, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateWorkOrderException>();
    }

    [Fact]
    public async Task Handle_WithDifferentCaseOrderNumber_DetectsDuplicate()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateWorkOrderCommandHandler(db);

        var cmd1 = new CreateWorkOrderCommand("WO-2026-CASE", ValidProductId, 10, FutureDueDate);
        await handler.Handle(cmd1, CancellationToken.None);

        var cmd2 = new CreateWorkOrderCommand("  wo-2026-case  ", ValidProductId, 20, FutureDueDate);

        var act = async () => await handler.Handle(cmd2, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateWorkOrderException>();
    }

    [Fact]
    public async Task Handle_PersistsAllFields()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new CreateWorkOrderCommandHandler(db);

        var cmd = new CreateWorkOrderCommand("WO-2026-100", ValidProductId, 42, FutureDueDate);
        await handler.Handle(cmd, CancellationToken.None);

        var wo = db.WorkOrders.Single();
        wo.OrderNumber.Should().Be("WO-2026-100");
        wo.ProductId.Should().Be(ValidProductId);
        wo.PlannedQuantity.Should().Be(42);
        wo.DueDate.Should().Be(FutureDueDate);
        wo.IsClosed.Should().BeFalse();
    }
}
