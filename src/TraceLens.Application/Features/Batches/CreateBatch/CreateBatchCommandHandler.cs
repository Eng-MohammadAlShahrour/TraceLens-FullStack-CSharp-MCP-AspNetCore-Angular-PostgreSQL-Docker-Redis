using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Domain.Entities;
using TraceLens.Domain.ValueObjects;

namespace TraceLens.Application.Features.Batches.CreateBatch;

public sealed class CreateBatchCommandHandler
    : IRequestHandler<CreateBatchCommand, CreateBatchResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IBatchNotifier _notifier;
    private readonly IDistributedCache _cache;

    // Cache keys invalidated on create — covers Batches page (s20) + Dashboard (s100)
    private static readonly string[] ListCacheKeys =
    {
        "batches:list:p1:s20",
        "batches:list:p1:s100",
        "batches:list:p1:s5",
    };

    public CreateBatchCommandHandler(
        IApplicationDbContext context,
        IBatchNotifier notifier,
        IDistributedCache cache)
    {
        _context = context;
        _notifier = notifier;
        _cache = cache;
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

        // Invalidate list cache (best-effort)
        foreach (var key in ListCacheKeys)
        {
            try { await _cache.RemoveAsync(key, cancellationToken); }
            catch { /* Redis down — ignore */ }
        }

        await _notifier.NotifyBatchCreatedAsync(batch.Id, batch.BatchNumber.Value,
            batch.Status.ToString(), cancellationToken);

        return new CreateBatchResponse(batch.Id, batch.BatchNumber.Value,
            batch.Status.ToString(), batch.CreatedAt);
    }
}
