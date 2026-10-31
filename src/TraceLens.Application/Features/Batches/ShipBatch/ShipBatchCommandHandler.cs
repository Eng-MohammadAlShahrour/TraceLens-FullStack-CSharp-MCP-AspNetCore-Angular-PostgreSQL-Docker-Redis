using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Application.Features.Batches.GetBatchById;

namespace TraceLens.Application.Features.Batches.ShipBatch;

public sealed class ShipBatchCommandHandler : IRequestHandler<ShipBatchCommand, BatchDto>
{
    private readonly IApplicationDbContext _context;

    public ShipBatchCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BatchDto> Handle(ShipBatchCommand request, CancellationToken ct)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new InvalidOperationException($"Batch {request.Id} not found.");

        batch.Ship();
        await _context.SaveChangesAsync(ct);

        return new BatchDto(batch.Id, batch.BatchNumber.Value, batch.Status.ToString(),
            batch.ProducedQuantity.Value, batch.ProducedQuantity.Unit,
            batch.WorkOrderId, batch.ProductId,
            batch.CreatedAt, batch.StartedAt, batch.CompletedAt, batch.Notes);
    }
}
