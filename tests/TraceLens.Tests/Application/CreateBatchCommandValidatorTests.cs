using FluentAssertions;
using TraceLens.Application.Features.Batches.CreateBatch;

namespace TraceLens.Tests.Application;

public class CreateBatchCommandValidatorTests
{
    private readonly CreateBatchCommandValidator _validator = new();

    private static CreateBatchCommand ValidCommand() => new(
        BatchNumber: "B-001",
        WorkOrderId: Guid.NewGuid(),
        ProductId: Guid.NewGuid(),
        ProducedQuantity: 100,
        ProducedQuantityUnit: "pcs");

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyBatchNumber_FailsValidation()
    {
        var cmd = ValidCommand() with { BatchNumber = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BatchNumber");
    }

    [Fact]
    public void BatchNumberTooLong_FailsValidation()
    {
        var cmd = ValidCommand() with { BatchNumber = new string('B', 51) };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BatchNumber");
    }

    [Fact]
    public void EmptyWorkOrderId_FailsValidation()
    {
        var cmd = ValidCommand() with { WorkOrderId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WorkOrderId");
    }

    [Fact]
    public void EmptyProductId_FailsValidation()
    {
        var cmd = ValidCommand() with { ProductId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
    }

    [Fact]
    public void ZeroQuantity_FailsValidation()
    {
        var cmd = ValidCommand() with { ProducedQuantity = 0 };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProducedQuantity");
    }

    [Fact]
    public void NegativeQuantity_FailsValidation()
    {
        var cmd = ValidCommand() with { ProducedQuantity = -5 };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProducedQuantity");
    }

    [Fact]
    public void EmptyUnit_FailsValidation()
    {
        var cmd = ValidCommand() with { ProducedQuantityUnit = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProducedQuantityUnit");
    }
}