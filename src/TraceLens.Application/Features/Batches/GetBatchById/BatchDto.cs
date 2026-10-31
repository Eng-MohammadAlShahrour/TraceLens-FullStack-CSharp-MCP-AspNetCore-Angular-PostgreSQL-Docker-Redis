namespace TraceLens.Application.Features.Batches.GetBatchById;

public sealed record BatchDto(
    Guid Id,
    string BatchNumber,
    string Status,
    decimal ProducedQuantity,
    string ProducedQuantityUnit,
    Guid WorkOrderId,
    Guid ProductId,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Notes
);
