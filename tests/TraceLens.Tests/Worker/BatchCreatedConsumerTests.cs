using System.Text.Json;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TraceLens.Application.Common.Events;
using TraceLens.Infrastructure.Persistence;
using TraceLens.Worker.Consumers;

namespace TraceLens.Tests.Worker;

public class BatchCreatedConsumerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"worker-created-{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Consume_CreatesAuditLogWithCorrectFields()
    {
        await using var db = CreateDb();
        var batchId = Guid.NewGuid();
        var evt = new BatchCreatedIntegrationEvent(
            batchId, "B-001", Guid.NewGuid(), Guid.NewGuid(), 50m, "pcs", "Created", DateTime.UtcNow);

        var context = Substitute.For<ConsumeContext<BatchCreatedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        var consumer = new BatchCreatedConsumer(db, Substitute.For<ILogger<BatchCreatedConsumer>>());
        await consumer.Consume(context);

        var log = await db.AuditLogs.SingleAsync();
        log.EventType.Should().Be("BatchCreatedIntegrationEvent");
        log.AggregateId.Should().Be(batchId);
        log.AggregateType.Should().Be("Batch");
        log.BatchNumber.Should().Be("B-001");
        log.NewStatus.Should().Be("Created");
        log.PreviousStatus.Should().BeNull();
        log.Reason.Should().BeNull();
    }

    [Fact]
    public async Task Consume_SavesPayloadAsJson()
    {
        await using var db = CreateDb();
        var evt = new BatchCreatedIntegrationEvent(
            Guid.NewGuid(), "B-JSON", Guid.NewGuid(), Guid.NewGuid(), 10m, "kg", "Created", DateTime.UtcNow);

        var context = Substitute.For<ConsumeContext<BatchCreatedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        var consumer = new BatchCreatedConsumer(db, Substitute.For<ILogger<BatchCreatedConsumer>>());
        await consumer.Consume(context);

        var log = await db.AuditLogs.SingleAsync();
        log.Payload.Should().Contain("B-JSON");
        Action act = () => JsonDocument.Parse(log.Payload);
        act.Should().NotThrow();
    }
}
