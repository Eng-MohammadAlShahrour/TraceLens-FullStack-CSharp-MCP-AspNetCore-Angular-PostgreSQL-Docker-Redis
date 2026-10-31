namespace TraceLens.Domain.ValueObjects;

public sealed record BatchNumber
{
    public string Value { get; }

    private BatchNumber(string value) => Value = value;

    public static BatchNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Batch number cannot be empty.", nameof(value));
        if (value.Length > 50)
            throw new ArgumentException("Batch number cannot exceed 50 characters.", nameof(value));
        return new BatchNumber(value.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;

    public static implicit operator string(BatchNumber bn) => bn.Value;
}
