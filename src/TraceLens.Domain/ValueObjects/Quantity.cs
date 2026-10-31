namespace TraceLens.Domain.ValueObjects;

public sealed record Quantity
{
    public decimal Value { get; }
    public string Unit { get; }

    private Quantity(decimal value, string unit)
    {
        Value = value;
        Unit = unit;
    }

    public static Quantity Create(decimal value, string unit)
    {
        if (value < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(value));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit is required.", nameof(unit));
        return new Quantity(value, unit.Trim().ToLowerInvariant());
    }

    public override string ToString() => $"{Value} {Unit}";
}
