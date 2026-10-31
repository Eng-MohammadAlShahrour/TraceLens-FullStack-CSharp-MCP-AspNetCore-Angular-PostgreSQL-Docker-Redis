using TraceLens.Domain.Common;
using TraceLens.Domain.Enums;
using TraceLens.Domain.Events;
using TraceLens.Domain.ValueObjects;

namespace TraceLens.Domain.Entities;

public sealed class Batch : AggregateRoot
{
    public BatchNumber BatchNumber { get; private set; } = null!;
    public Guid WorkOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity ProducedQuantity { get; private set; } = null!;
    public BatchStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<MaterialLot> _materialLots = new();
    public IReadOnlyCollection<MaterialLot> MaterialLots => _materialLots.AsReadOnly();

    private Batch() { }

    public static Batch Create(
        BatchNumber batchNumber,
        Guid workOrderId,
        Guid productId,
        Quantity producedQuantity)
    {
        if (workOrderId == Guid.Empty)
            throw new ArgumentException("Work order ID is required.", nameof(workOrderId));
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.", nameof(productId));

        var batch = new Batch
        {
            BatchNumber = batchNumber,
            WorkOrderId = workOrderId,
            ProductId = productId,
            ProducedQuantity = producedQuantity,
            Status = BatchStatus.Created
        };

        batch.RaiseDomainEvent(new BatchCreatedDomainEvent(batch.Id, batch.BatchNumber.Value));
        return batch;
    }

    public void StartProduction()
    {
        if (Status != BatchStatus.Created)
            throw new InvalidOperationException($"Cannot start production from status '{Status}'.");

        Status = BatchStatus.InProduction;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkQCCompleted(QualityCheckResult result)
    {
        if (Status != BatchStatus.InProduction && Status != BatchStatus.QCPending)
            throw new InvalidOperationException($"Cannot complete QC from status '{Status}'.");

        if (result == QualityCheckResult.Passed)
        {
            Status = BatchStatus.QCPassed;
            RaiseDomainEvent(new QualityCheckPassedDomainEvent(Id, BatchNumber.Value));
        }
        else if (result == QualityCheckResult.Failed)
        {
            Status = BatchStatus.QCFailed;
        }
        else
        {
            throw new InvalidOperationException("QC result must be Passed or Failed.");
        }

        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Quarantine(string reason)
    {
        if (Status == BatchStatus.Shipped)
            throw new InvalidOperationException("Cannot quarantine a shipped batch.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Quarantine reason is required.", nameof(reason));

        Status = BatchStatus.Quarantined;
        Notes = reason;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new BatchQuarantinedDomainEvent(Id, BatchNumber.Value, reason));
    }

    public void Ship()
    {
        if (Status != BatchStatus.QCPassed)
            throw new InvalidOperationException("Cannot ship a batch that has not passed QC.");

        Status = BatchStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMaterialLot(MaterialLot lot)
    {
        if (Status != BatchStatus.Created && Status != BatchStatus.InProduction)
            throw new InvalidOperationException("Cannot add materials after production ended.");

        _materialLots.Add(lot);
        UpdatedAt = DateTime.UtcNow;
    }
}
