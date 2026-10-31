using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Interfaces;

namespace TraceLens.Application.Features.Batches.GetBatchById;

public sealed class GetBatchByIdQueryHandler
    : IRequestHandler<GetBatchByIdQuery, BatchDto?>
{
    private readonly IApplicationDbContext _context;

    public GetBatchByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BatchDto?> Handle(
        GetBatchByIdQuery request,
        CancellationToken cancellationToken)
    {
        var batch = await _context.Batches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (batch is null)
            return null;

        return new BatchDto(
            batch.Id,
            batch.BatchNumber.Value,
            batch.Status.ToString(),
            batch.ProducedQuantity.Value,
            batch.ProducedQuantity.Unit,
            batch.WorkOrderId,
            batch.ProductId,
            batch.CreatedAt,
            batch.StartedAt,
            batch.CompletedAt,
            batch.Notes);
    }
}
