using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Events;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Application.Features.Batches.GetBatchById;

namespace TraceLens.Application.Features.Batches.QuarantineBatch;

public sealed class QuarantineBatchCommandHandler : IRequestHandler<QuarantineBatchCommand, BatchDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IBatchNotifier _notifier;
    private readonly IEventPublisher _publisher;

    public QuarantineBatchCommandHandler(
        IApplicationDbContext context,
        IBatchNotifier notifier,
        IEventPublisher publisher)
    {
        _context = context;
        _notifier = notifier;
        _publisher = publisher;
    }

    public async Task<BatchDto> Handle(QuarantineBatchCommand request, CancellationToken ct)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new InvalidOperationException($"Batch {request.Id} not found.");

        var previousStatus = batch.Status.ToString();

        batch.Quarantine(request.Reason);
        await _context.SaveChangesAsync(ct);

        await _notifier.NotifyBatchStatusChangedAsync(
            batch.Id, batch.BatchNumber.Value, batch.Status.ToString(), ct);

        await _publisher.PublishAsync(new BatchStatusChangedIntegrationEvent(
            batch.Id,
            batch.BatchNumber.Value,
            previousStatus,
            batch.Status.ToString(),
            request.Reason,           // ← Reason included ONLY here
            DateTime.UtcNow),
            ct);

        return new BatchDto(batch.Id, batch.BatchNumber.Value, batch.Status.ToString(),
            batch.ProducedQuantity.Value, batch.ProducedQuantity.Unit,
            batch.WorkOrderId, batch.ProductId,
            batch.CreatedAt, batch.StartedAt, batch.CompletedAt, batch.Notes);
    }
}
