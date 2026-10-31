using FluentAssertions;
using TraceLens.Domain.ValueObjects;

namespace TraceLens.Tests.Domain;

public class BatchNumberTests
{
    [Fact]
    public void Create_WithValidValue_ReturnsBatchNumber()
    {
        var bn = BatchNumber.Create("B-2026-001");
        bn.Value.Should().Be("B-2026-001");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var bn = BatchNumber.Create("  B-2026-001  ");
        bn.Value.Should().Be("B-2026-001");
    }

    [Fact]
    public void Create_NormalizesToUppercase()
    {
        var bn = BatchNumber.Create("b-2026-001");
        bn.Value.Should().Be("B-2026-001");
    }

    [Fact]
    public void Create_WithEmpty_Throws()
    {
        var act = () => BatchNumber.Create("");
        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void Create_WithWhitespace_Throws()
    {
        var act = () => BatchNumber.Create("   ");
        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void Create_TooLong_Throws()
    {
        var longValue = new string('B', 51);
        var act = () => BatchNumber.Create(longValue);
        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void TwoBatchNumbers_WithSameValue_AreEqual()
    {
        var a = BatchNumber.Create("B-001");
        var b = BatchNumber.Create("b-001");
        a.Should().Be(b);
    }

    [Fact]
    public void ImplicitOperator_ReturnsString()
    {
        var bn = BatchNumber.Create("B-001");
        string value = bn;
        value.Should().Be("B-001");
    }
}