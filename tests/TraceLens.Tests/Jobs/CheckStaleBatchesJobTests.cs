using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TraceLens.API.Jobs;
using TraceLens.Domain.Entities;
using TraceLens.Domain.ValueObjects;
using TraceLens.Tests.Helpers;

namespace TraceLens.Tests.Jobs;

public class CheckStaleBatchesJobTests
{
    [Fact]
    public async Task Run_WithStaleBatch_Succeeds()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var stale = Batch.Create(
            BatchNumber.Create("B-STALE-1"),
            Guid.NewGuid(), Guid.NewGuid(),
            Quantity.Create(10, "pcs"));
        stale.StartProduction();
        stale.ClearDomainEvents();

        typeof(Batch).GetProperty("StartedAt")!
            .SetValue(stale, DateTime.UtcNow.AddHours(-25));

        db.Batches.Add(stale);
        await db.SaveChangesAsync();

        var job = new CheckStaleBatchesJob(db, NullLogger<CheckStaleBatchesJob>.Instance);
        var act = async () => await job.Run(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Run_WithNoStaleBatches_Succeeds()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var fresh = Batch.Create(
            BatchNumber.Create("B-FRESH-1"),
            Guid.NewGuid(), Guid.NewGuid(),
            Quantity.Create(10, "pcs"));
        fresh.StartProduction();
        fresh.ClearDomainEvents();
        db.Batches.Add(fresh);
        await db.SaveChangesAsync();

        var job = new CheckStaleBatchesJob(db, NullLogger<CheckStaleBatchesJob>.Instance);
        var act = async () => await job.Run(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
