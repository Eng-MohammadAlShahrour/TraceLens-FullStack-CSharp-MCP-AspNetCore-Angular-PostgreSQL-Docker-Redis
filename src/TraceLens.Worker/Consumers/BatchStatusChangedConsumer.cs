using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using TraceLens.Application.Common.Events;
using TraceLens.Domain.Entities;
using TraceLens.Infrastructure.Persistence;

namespace TraceLens.Worker.Consumers;

/// <summary>
/// Consumes BatchStatusChangedIntegrationEvent from RabbitMQ and persists an audit log row.
/// Covers 5 transitions: StartProduction, PassQC, FailQC, Ship, Quarantine.
/// Future: hook Slack webhook here for QCFailed / Quarantined.
/// </summary>
public sealed class BatchStatusChangedConsumer : IConsumer<BatchStatusChangedIntegrationEvent>
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<BatchStatusChangedConsumer> _logger;

    public BatchStatusChangedConsumer(ApplicationDbContext db, ILogger<BatchStatusChangedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BatchStatusChangedIntegrationEvent> context)
    {
        var evt = context.Message;
        var payload = JsonSerializer.Serialize(evt);

        var log = AuditLog.Create(
            eventType: nameof(BatchStatusChangedIntegrationEvent),
            aggregateId: evt.BatchId,
            aggregateType: "Batch",
            batchNumber: evt.BatchNumber,
            payload: payload,
            previousStatus: evt.PreviousStatus,
            newStatus: evt.NewStatus,
            reason: evt.Reason);

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Audit: {EventType} batch={BatchNumber} {Prev} -> {New}{Reason}",
            log.EventType, log.BatchNumber, log.PreviousStatus, log.NewStatus,
            string.IsNullOrEmpty(log.Reason) ? "" : $" (reason: {log.Reason})");
    }
}
