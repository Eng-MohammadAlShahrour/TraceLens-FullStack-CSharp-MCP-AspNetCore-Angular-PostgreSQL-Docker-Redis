using FluentAssertions;
using TraceLens.Application.Features.Activity.GetActivity;
using TraceLens.Domain.Entities;
using TraceLens.Tests.Helpers;

namespace TraceLens.Tests.Application;

public class GetActivityQueryHandlerTests
{
    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new GetActivityQueryHandler(db);

        var response = await handler.Handle(new GetActivityQuery(100), CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithMultipleLogs_OrdersByCreatedAtDescending()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var oldLog = AuditLog.Create("BatchCreatedIntegrationEvent", Guid.NewGuid(), "Batch", "B-OLD", "{}", newStatus: "Created");
        var newLog = AuditLog.Create("BatchCreatedIntegrationEvent", Guid.NewGuid(), "Batch", "B-NEW", "{}", newStatus: "Created");

        db.AuditLogs.Add(oldLog);
        await db.SaveChangesAsync();
        await Task.Delay(50);  // ensure distinct timestamps
        db.AuditLogs.Add(newLog);
        await db.SaveChangesAsync();

        var handler = new GetActivityQueryHandler(db);
        var response = await handler.Handle(new GetActivityQuery(100), CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.Items[0].BatchNumber.Should().Be("B-NEW");
        response.Items[1].BatchNumber.Should().Be("B-OLD");
    }

    [Fact]
    public async Task Handle_RespectsLimit()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        for (int i = 0; i < 5; i++)
        {
            db.AuditLogs.Add(AuditLog.Create(
                "BatchCreatedIntegrationEvent", Guid.NewGuid(), "Batch", $"B-{i}", "{}", newStatus: "Created"));
        }
        await db.SaveChangesAsync();

        var handler = new GetActivityQueryHandler(db);
        var response = await handler.Handle(new GetActivityQuery(2), CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.TotalCount.Should().Be(5);  // total count ignores limit
    }
}
