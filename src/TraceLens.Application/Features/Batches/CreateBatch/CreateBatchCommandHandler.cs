using MediatR;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Domain.Entities;
using TraceLens.Domain.ValueObjects;

namespace TraceLens.Application.Features.Batches.CreateBatch;

public sealed class CreateBatchCommandHandler
    : IRequestHandler<CreateBatchCommand, CreateBatchResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateBatchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateBatchResponse> Handle(
        CreateBatchCommand request,
        CancellationToken cancellationToken)
    {
        var batch = Batch.Create(
            BatchNumber.Create(request.BatchNumber),
            request.WorkOrderId,
            request.ProductId,
            Quantity.Create(request.ProducedQuantity, request.ProducedQuantityUnit));

        _context.Batches.Add(batch);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateBatchResponse(
            batch.Id,
            batch.BatchNumber.Value,
            batch.Status.ToString(),
            batch.CreatedAt);
    }
}
