using FluentAssertions;
using TraceLens.Domain.Entities;

namespace TraceLens.Tests.Domain;

public class WorkOrderTests
{
    private static readonly Guid ValidProductId = Guid.NewGuid();
    private static readonly DateTime FutureDueDate = DateTime.UtcNow.Date.AddDays(30);

    // ── Create ──

    [Fact]
    public void Create_WithValidData_ReturnsWorkOrder()
    {
        var wo = WorkOrder.Create("WO-2026-001", ValidProductId, 100, FutureDueDate);

        wo.OrderNumber.Should().Be("WO-2026-001");
        wo.ProductId.Should().Be(ValidProductId);
        wo.PlannedQuantity.Should().Be(100);
        wo.DueDate.Should().Be(FutureDueDate);
        wo.IsClosed.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrderNumber_ThrowsArgumentException(string? orderNumber)
    {
        Action act = () => WorkOrder.Create(orderNumber!, ValidProductId, 100, FutureDueDate);

        act.Should().Throw<ArgumentException>().WithParameterName("orderNumber");
    }

    [Fact]
    public void Create_WithEmptyProductId_ThrowsArgumentException()
    {
        Action act = () => WorkOrder.Create("WO-2026-001", Guid.Empty, 100, FutureDueDate);

        act.Should().Throw<ArgumentException>().WithParameterName("productId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithNonPositiveQuantity_ThrowsArgumentException(int quantity)
    {
        Action act = () => WorkOrder.Create("WO-2026-001", ValidProductId, quantity, FutureDueDate);

        act.Should().Throw<ArgumentException>().WithParameterName("plannedQuantity");
    }

    [Fact]
    public void Create_WithPastDueDate_ThrowsArgumentException()
    {
        var pastDate = DateTime.UtcNow.Date.AddDays(-1);

        Action act = () => WorkOrder.Create("WO-2026-001", ValidProductId, 100, pastDate);

        act.Should().Throw<ArgumentException>().WithParameterName("dueDate");
    }

    [Fact]
    public void Create_WithTodayDueDate_Succeeds()
    {
        var today = DateTime.UtcNow.Date;

        var wo = WorkOrder.Create("WO-2026-001", ValidProductId, 100, today);

        wo.DueDate.Should().Be(today);
    }

    [Fact]
    public void Create_TrimsAndUppercasesOrderNumber()
    {
        var wo = WorkOrder.Create("  wo-2026-001  ", ValidProductId, 100, FutureDueDate);

        wo.OrderNumber.Should().Be("WO-2026-001");
    }

    // ── Close ──

    [Fact]
    public void Close_WithOpenOrder_ClosesItAndUpdatesTimestamp()
    {
        var wo = WorkOrder.Create("WO-2026-001", ValidProductId, 100, FutureDueDate);
        var beforeClose = DateTime.UtcNow;

        wo.Close();

        wo.IsClosed.Should().BeTrue();
        wo.UpdatedAt.Should().NotBeNull();
        wo.UpdatedAt!.Value.Should().BeOnOrAfter(beforeClose).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Close_WithAlreadyClosedOrder_ThrowsInvalidOperationException()
    {
        var wo = WorkOrder.Create("WO-2026-001", ValidProductId, 100, FutureDueDate);
        wo.Close();

        Action act = () => wo.Close();

        act.Should().Throw<InvalidOperationException>().WithMessage("*already closed*");
    }
}
