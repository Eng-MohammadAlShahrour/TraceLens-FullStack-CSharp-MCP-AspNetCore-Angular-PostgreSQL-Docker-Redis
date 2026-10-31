using FluentAssertions;
using TraceLens.Domain.ValueObjects;

namespace TraceLens.Tests.Domain;

public class QuantityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsQuantity()
    {
        var q = Quantity.Create(100, "pcs");
        q.Value.Should().Be(100);
        q.Unit.Should().Be("pcs");
    }

    [Fact]
    public void Create_WithZero_IsAllowed()
    {
        var q = Quantity.Create(0, "pcs");
        q.Value.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegative_Throws()
    {
        var act = () => Quantity.Create(-1, "pcs");
        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void Create_WithEmptyUnit_Throws()
    {
        var act = () => Quantity.Create(100, "");
        act.Should().Throw<ArgumentException>().WithParameterName("unit");
    }

    [Fact]
    public void Create_NormalizesUnitToLowercase()
    {
        var q = Quantity.Create(100, "PCS");
        q.Unit.Should().Be("pcs");
    }

    [Fact]
    public void ToString_ReturnsFormattedValue()
    {
        var q = Quantity.Create(100.5m, "kg");
        q.ToString().Should().Be("100.5 kg");
    }
}