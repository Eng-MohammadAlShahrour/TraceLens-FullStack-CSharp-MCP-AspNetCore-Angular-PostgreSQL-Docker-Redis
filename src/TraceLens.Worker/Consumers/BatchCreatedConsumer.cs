using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using TraceLens.Application.Common.Events;
using TraceLens.Domain.Entities;
using TraceLens.Infrastructure.Persistence;

namespace TraceLens.Worker.Consumers;

/// <summary>
/// Consumes BatchCreatedIntegrationEvent from RabbitMQ and persists an audit log row.
/// Runs in TraceLens.Worker (separate process) — writes to the same PostgreSQL database.
/// </summary>
public sealed class BatchCreatedConsumer : IConsumer<BatchCreatedIntegrationEvent>
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<BatchCreatedConsumer> _logger;

    public BatchCreatedConsumer(ApplicationDbContext db, ILogger<BatchCreatedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BatchCreatedIntegrationEvent> context)
    {
        var evt = context.Message;
        var payload = JsonSerializer.Serialize(evt);

        var log = AuditLog.Create(
            eventType: nameof(BatchCreatedIntegrationEvent),
            aggregateId: evt.BatchId,
            aggregateType: "Batch",
            batchNumber: evt.BatchNumber,
            payload: payload,
            newStatus: evt.Status);

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Audit: {EventType} batch={BatchNumber} status={Status}",
            log.EventType, log.BatchNumber, log.NewStatus);
    }
}
