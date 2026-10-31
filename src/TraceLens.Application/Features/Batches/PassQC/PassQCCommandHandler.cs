using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Application.Features.Batches.GetBatchById;
using TraceLens.Domain.Enums;

namespace TraceLens.Application.Features.Batches.PassQC;

public sealed class PassQCCommandHandler : IRequestHandler<PassQCCommand, BatchDto>
{
    private readonly IApplicationDbContext _context;

    public PassQCCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BatchDto> Handle(PassQCCommand request, CancellationToken ct)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new InvalidOperationException($"Batch {request.Id} not found.");

        batch.MarkQCCompleted(QualityCheckResult.Passed);
        await _context.SaveChangesAsync(ct);

        return new BatchDto(batch.Id, batch.BatchNumber.Value, batch.Status.ToString(),
            batch.ProducedQuantity.Value, batch.ProducedQuantity.Unit,
            batch.WorkOrderId, batch.ProductId,
            batch.CreatedAt, batch.StartedAt, batch.CompletedAt, batch.Notes);
    }
}
