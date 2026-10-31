namespace TraceLens.Application.Features.Batches.GetBatchesList;

public sealed record GetBatchesListResponse(
    List<BatchListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public sealed record BatchListItemDto(
    Guid Id,
    string BatchNumber,
    string Status,
    decimal ProducedQuantity,
    string Unit,
    DateTime CreatedAt
);
