using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using TraceLens.Application.Common.Events;
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
    private readonly IEventPublisher _publisher;

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
        IDistributedCache cache,
        IEventPublisher publisher)
    {
        _context = context;
        _notifier = notifier;
        _cache = cache;
        _publisher = publisher;
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

        // SignalR notification (UI — fire and forget)
        await _notifier.NotifyBatchCreatedAsync(batch.Id, batch.BatchNumber.Value,
            batch.Status.ToString(), cancellationToken);

        // Publish integration event (durable — audit log, Slack, etc. via RabbitMQ)
        await _publisher.PublishAsync(new BatchCreatedIntegrationEvent(
            batch.Id,
            batch.BatchNumber.Value,
            batch.WorkOrderId,
            batch.ProductId,
            batch.ProducedQuantity.Value,
            batch.ProducedQuantity.Unit,
            batch.Status.ToString(),
            batch.CreatedAt),
            cancellationToken);

        return new CreateBatchResponse(batch.Id, batch.BatchNumber.Value,
            batch.Status.ToString(), batch.CreatedAt);
    }
}
