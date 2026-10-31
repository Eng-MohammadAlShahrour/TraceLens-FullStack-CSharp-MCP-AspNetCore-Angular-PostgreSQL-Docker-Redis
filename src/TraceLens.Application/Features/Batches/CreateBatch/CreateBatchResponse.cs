namespace TraceLens.Application.Features.Batches.CreateBatch;

public sealed record CreateBatchResponse(
    Guid Id,
    string BatchNumber,
    string Status,
    DateTime CreatedAt
);
