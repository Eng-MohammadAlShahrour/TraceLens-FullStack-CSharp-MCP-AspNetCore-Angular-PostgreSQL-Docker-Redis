using TraceLens.Domain.Common;
using TraceLens.Domain.Enums;
using TraceLens.Domain.ValueObjects;

namespace TraceLens.Domain.Entities;

public sealed class MaterialLot : BaseEntity
{
    public string LotNumber { get; private set; } = null!;
    public Guid SupplierId { get; private set; }
    public MaterialType Type { get; private set; }
    public Quantity Quantity { get; private set; } = null!;
    public DateTime ReceivedAt { get; private set; }
    public string? CertificateOfAnalysis { get; private set; }

    private MaterialLot() { }

    public static MaterialLot Create(
        string lotNumber,
        Guid supplierId,
        MaterialType type,
        Quantity quantity,
        string? certificateOfAnalysis = null)
    {
        if (string.IsNullOrWhiteSpace(lotNumber))
            throw new ArgumentException("Lot number is required.", nameof(lotNumber));
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID is required.", nameof(supplierId));

        return new MaterialLot
        {
            LotNumber = lotNumber.Trim().ToUpperInvariant(),
            SupplierId = supplierId,
            Type = type,
            Quantity = quantity,
            ReceivedAt = DateTime.UtcNow,
            CertificateOfAnalysis = certificateOfAnalysis
        };
    }
}
