namespace TraceLens.Application.Common.Events;

/// <summary>
/// Published when a new batch is created.
/// Consumed by TraceLens.Worker to write audit log + broadcast to SignalR via activity stream.
/// </summary>
public sealed record BatchCreatedIntegrationEvent(
    Guid BatchId,
    string BatchNumber,
    Guid WorkOrderId,
    Guid ProductId,
    decimal ProducedQuantity,
    string Unit,
    string Status,
    DateTime CreatedAt);

/// <summary>
/// Published on every batch status transition (StartProduction, PassQC, FailQC, Ship, Quarantine).
/// Consumers filter by NewStatus to react to specific transitions (e.g. QCFailed -> Slack alert).
/// Reason is set only for Quarantine transitions; null otherwise.
/// </summary>
public sealed record BatchStatusChangedIntegrationEvent(
    Guid BatchId,
    string BatchNumber,
    string PreviousStatus,
    string NewStatus,
    string? Reason,
    DateTime ChangedAt);
