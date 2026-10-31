using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Application.Features.Batches.GetBatchById;

namespace TraceLens.Application.Features.Batches.QuarantineBatch;

public sealed class QuarantineBatchCommandHandler : IRequestHandler<QuarantineBatchCommand, BatchDto>
{
    private readonly IApplicationDbContext _context;

    public QuarantineBatchCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BatchDto> Handle(QuarantineBatchCommand request, CancellationToken ct)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new InvalidOperationException($"Batch {request.Id} not found.");

        batch.Quarantine(request.Reason);
        await _context.SaveChangesAsync(ct);

        return new BatchDto(batch.Id, batch.BatchNumber.Value, batch.Status.ToString(),
            batch.ProducedQuantity.Value, batch.ProducedQuantity.Unit,
            batch.WorkOrderId, batch.ProductId,
            batch.CreatedAt, batch.StartedAt, batch.CompletedAt, batch.Notes);
    }
}
