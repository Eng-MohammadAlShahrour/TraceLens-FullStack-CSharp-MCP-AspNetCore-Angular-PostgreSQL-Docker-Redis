using FluentAssertions;
using TraceLens.Domain.Entities;
using TraceLens.Domain.Enums;
using TraceLens.Domain.Events;
using TraceLens.Domain.ValueObjects;

namespace TraceLens.Tests.Domain;

public class BatchTests
{
    private static Batch CreateValidBatch() => Batch.Create(
        BatchNumber.Create("B-001"),
        Guid.NewGuid(),
        Guid.NewGuid(),
        Quantity.Create(100, "pcs"));

    // ── Creation ─────────────────────────────────
    [Fact]
    public void Create_WithValidData_ReturnsCreatedBatch()
    {
        var batch = CreateValidBatch();
        batch.Status.Should().Be(BatchStatus.Created);
        batch.BatchNumber.Value.Should().Be("B-001");
        batch.ProducedQuantity.Value.Should().Be(100);
        batch.ProducedQuantity.Unit.Should().Be("pcs");
    }

    [Fact]
    public void Create_WithEmptyWorkOrderId_Throws()
    {
        var act = () => Batch.Create(BatchNumber.Create("B-001"), Guid.Empty, Guid.NewGuid(), Quantity.Create(1, "pcs"));
        act.Should().Throw<ArgumentException>().WithParameterName("workOrderId");
    }

    [Fact]
    public void Create_WithEmptyProductId_Throws()
    {
        var act = () => Batch.Create(BatchNumber.Create("B-001"), Guid.NewGuid(), Guid.Empty, Quantity.Create(1, "pcs"));
        act.Should().Throw<ArgumentException>().WithParameterName("productId");
    }

    [Fact]
    public void Create_RaisesBatchCreatedEvent()
    {
        var batch = CreateValidBatch();
        batch.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BatchCreatedDomainEvent>();
    }

    // ── StartProduction ──────────────────────────
    [Fact]
    public void StartProduction_FromCreated_ChangesStatus()
    {
        var batch = CreateValidBatch();
        batch.StartProduction();
        batch.Status.Should().Be(BatchStatus.InProduction);
        batch.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public void StartProduction_FromInProduction_Throws()
    {
        var batch = CreateValidBatch();
        batch.StartProduction();
        var act = () => batch.StartProduction();
        act.Should().Throw<InvalidOperationException>().WithMessage("*Cannot start production*");
    }

    // ── MarkQCCompleted ──────────────────────────
    [Fact]
    public void MarkQCCompleted_Passed_ChangesStatusAndRaisesEvent()
    {
        var batch = CreateValidBatch();
        batch.StartProduction();
        batch.ClearDomainEvents();

        batch.MarkQCCompleted(QualityCheckResult.Passed);

        batch.Status.Should().Be(BatchStatus.QCPassed);
        batch.CompletedAt.Should().NotBeNull();
        batch.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<QualityCheckPassedDomainEvent>();
    }

    [Fact]
    public void MarkQCCompleted_Failed_ChangesStatus()
    {
        var batch = CreateValidBatch();
        batch.StartProduction();
        batch.MarkQCCompleted(QualityCheckResult.Failed);
        batch.Status.Should().Be(BatchStatus.QCFailed);
    }

    [Fact]
    public void MarkQCCompleted_FromCreated_Throws()
    {
        var batch = CreateValidBatch();
        var act = () => batch.MarkQCCompleted(QualityCheckResult.Passed);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Cannot complete QC*");
    }

    [Fact]
    public void MarkQCCompleted_WithPending_Throws()
    {
        var batch = CreateValidBatch();
        batch.StartProduction();
        var act = () => batch.MarkQCCompleted(QualityCheckResult.Pending);
        act.Should().Throw<InvalidOperationException>().WithMessage("*must be Passed or Failed*");
    }

    // ── Quarantine ───────────────────────────────
    [Fact]
    public void Quarantine_SetsStatusAndNotes()
    {
        var batch = CreateValidBatch();
        batch.StartProduction();
        batch.Quarantine("Dimension out of tolerance");
        batch.Status.Should().Be(BatchStatus.Quarantined);
        batch.Notes.Should().Be("Dimension out of tolerance");
    }

    [Fact]
    public void Quarantine_ShippedBatch_Throws()
    {
        var batch = CreateValidBatch();
        batch.StartProduction();
        batch.MarkQCCompleted(QualityCheckResult.Passed);
        batch.Ship();

        var act = () => batch.Quarantine("Late issue");
        act.Should().Throw<InvalidOperationException>().WithMessage("*Cannot quarantine*");
    }

    [Fact]
    public void Quarantine_WithEmptyReason_Throws()
    {
        var batch = CreateValidBatch();
        var act = () => batch.Quarantine("");
        act.Should().Throw<ArgumentException>().WithParameterName("reason");
    }

    // ── Ship ─────────────────────────────────────
    [Fact]
    public void Ship_FromQCPassed_ChangesStatus()
    {
        var batch = CreateValidBatch();
        batch.StartProduction();
        batch.MarkQCCompleted(QualityCheckResult.Passed);
        batch.Ship();
        batch.Status.Should().Be(BatchStatus.Shipped);
    }

    [Fact]
    public void Ship_FromCreated_Throws()
    {
        var batch = CreateValidBatch();
        var act = () => batch.Ship();
        act.Should().Throw<InvalidOperationException>().WithMessage("*not passed QC*");
    }

    // ── AddMaterialLot ───────────────────────────
    [Fact]
    public void AddMaterialLot_ToCreated_Works()
    {
        var batch = CreateValidBatch();
        var lot = MaterialLot.Create("LOT-001", Guid.NewGuid(), MaterialType.Raw, Quantity.Create(50, "kg"));
        batch.AddMaterialLot(lot);
        batch.MaterialLots.Should().ContainSingle();
    }

    [Fact]
    public void AddMaterialLot_AfterShipped_Throws()
    {
        var batch = CreateValidBatch();
        batch.StartProduction();
        batch.MarkQCCompleted(QualityCheckResult.Passed);
        batch.Ship();

        var lot = MaterialLot.Create("LOT-002", Guid.NewGuid(), MaterialType.Raw, Quantity.Create(10, "kg"));
        var act = () => batch.AddMaterialLot(lot);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Cannot add materials*");
    }
}