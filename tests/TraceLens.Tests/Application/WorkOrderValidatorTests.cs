using FluentAssertions;
using TraceLens.Application.Features.WorkOrders.CreateWorkOrder;

namespace TraceLens.Tests.Application;

public class WorkOrderValidatorTests
{
    private readonly CreateWorkOrderCommandValidator _validator = new();
    private static readonly Guid ValidProductId = Guid.NewGuid();
    private static readonly DateTime FutureDueDate = DateTime.UtcNow.Date.AddDays(30);

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var cmd = new CreateWorkOrderCommand("WO-2026-001", ValidProductId, 100, FutureDueDate);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyOrderNumber_Fails(string orderNumber)
    {
        var cmd = new CreateWorkOrderCommand(orderNumber, ValidProductId, 100, FutureDueDate);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.OrderNumber));
    }

    [Fact]
    public void Validate_WithOrderNumberTooLong_Fails()
    {
        var longNumber = new string('A', 51);
        var cmd = new CreateWorkOrderCommand(longNumber, ValidProductId, 100, FutureDueDate);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.OrderNumber));
    }

    [Fact]
    public void Validate_WithEmptyProductId_Fails()
    {
        var cmd = new CreateWorkOrderCommand("WO-2026-001", Guid.Empty, 100, FutureDueDate);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.ProductId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Validate_WithNonPositivePlannedQuantity_Fails(int quantity)
    {
        var cmd = new CreateWorkOrderCommand("WO-2026-001", ValidProductId, quantity, FutureDueDate);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.PlannedQuantity));
    }

    [Fact]
    public void Validate_WithTodayDueDate_Fails()
    {
        var cmd = new CreateWorkOrderCommand("WO-2026-001", ValidProductId, 100, DateTime.UtcNow.Date);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.DueDate));
    }
}
