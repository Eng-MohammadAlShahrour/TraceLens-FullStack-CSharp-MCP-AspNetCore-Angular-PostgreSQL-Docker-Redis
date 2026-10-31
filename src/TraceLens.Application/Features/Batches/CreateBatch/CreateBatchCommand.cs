using MediatR;

namespace TraceLens.Application.Features.Batches.CreateBatch;

public sealed record CreateBatchCommand(
    string BatchNumber,
    Guid WorkOrderId,
    Guid ProductId,
    decimal ProducedQuantity,
    string ProducedQuantityUnit
) : IRequest<CreateBatchResponse>;
