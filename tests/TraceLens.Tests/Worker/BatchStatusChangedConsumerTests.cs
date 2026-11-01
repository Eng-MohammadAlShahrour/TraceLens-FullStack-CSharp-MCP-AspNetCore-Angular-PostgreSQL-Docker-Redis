using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TraceLens.Application.Common.Events;
using TraceLens.Infrastructure.Persistence;
using TraceLens.Worker.Consumers;

namespace TraceLens.Tests.Worker;

public class BatchStatusChangedConsumerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"worker-status-{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Consume_WithTransition_SavesPreviousAndNewStatus()
    {
        await using var db = CreateDb();
        var batchId = Guid.NewGuid();
        var evt = new BatchStatusChangedIntegrationEvent(
            batchId, "B-TRANS", "InProduction", "QCPassed", null, DateTime.UtcNow);

        var context = Substitute.For<ConsumeContext<BatchStatusChangedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        var consumer = new BatchStatusChangedConsumer(db, Substitute.For<ILogger<BatchStatusChangedConsumer>>());
        await consumer.Consume(context);

        var log = await db.AuditLogs.SingleAsync();
        log.EventType.Should().Be("BatchStatusChangedIntegrationEvent");
        log.BatchNumber.Should().Be("B-TRANS");
        log.PreviousStatus.Should().Be("InProduction");
        log.NewStatus.Should().Be("QCPassed");
        log.Reason.Should().BeNull();
    }

    [Fact]
    public async Task Consume_WithQuarantineReason_SavesReason()
    {
        await using var db = CreateDb();
        var evt = new BatchStatusChangedIntegrationEvent(
            Guid.NewGuid(), "B-QUAR", "InProduction", "Quarantined", "Contamination detected", DateTime.UtcNow);

        var context = Substitute.For<ConsumeContext<BatchStatusChangedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        var consumer = new BatchStatusChangedConsumer(db, Substitute.For<ILogger<BatchStatusChangedConsumer>>());
        await consumer.Consume(context);

        var log = await db.AuditLogs.SingleAsync();
        log.NewStatus.Should().Be("Quarantined");
        log.Reason.Should().Be("Contamination detected");
    }
}
