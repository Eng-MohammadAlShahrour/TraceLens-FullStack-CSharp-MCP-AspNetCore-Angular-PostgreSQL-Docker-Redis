using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Events;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Application.Features.Batches.GetBatchById;

namespace TraceLens.Application.Features.Batches.StartProduction;

public sealed class StartProductionCommandHandler : IRequestHandler<StartProductionCommand, BatchDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IBatchNotifier _notifier;
    private readonly IEventPublisher _publisher;

    public StartProductionCommandHandler(
        IApplicationDbContext context,
        IBatchNotifier notifier,
        IEventPublisher publisher)
    {
        _context = context;
        _notifier = notifier;
        _publisher = publisher;
    }

    public async Task<BatchDto> Handle(StartProductionCommand request, CancellationToken ct)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new InvalidOperationException($"Batch {request.Id} not found.");

        var previousStatus = batch.Status.ToString();

        batch.StartProduction();
        await _context.SaveChangesAsync(ct);

        await _notifier.NotifyBatchStatusChangedAsync(
            batch.Id, batch.BatchNumber.Value, batch.Status.ToString(), ct);

        await _publisher.PublishAsync(new BatchStatusChangedIntegrationEvent(
            batch.Id,
            batch.BatchNumber.Value,
            previousStatus,
            batch.Status.ToString(),
            null,
            DateTime.UtcNow),
            ct);

        return new BatchDto(batch.Id, batch.BatchNumber.Value, batch.Status.ToString(),
            batch.ProducedQuantity.Value, batch.ProducedQuantity.Unit,
            batch.WorkOrderId, batch.ProductId,
            batch.CreatedAt, batch.StartedAt, batch.CompletedAt, batch.Notes);
    }
}
