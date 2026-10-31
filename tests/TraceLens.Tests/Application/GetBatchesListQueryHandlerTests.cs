using FluentAssertions;
using TraceLens.Application.Features.Batches.GetBatchesList;
using TraceLens.Domain.Entities;
using TraceLens.Domain.ValueObjects;
using TraceLens.Tests.Helpers;

namespace TraceLens.Tests.Application;

public class GetBatchesListQueryHandlerTests
{
    private static Batch MakeBatch(string number, int qty = 100) =>
        Batch.Create(BatchNumber.Create(number), Guid.NewGuid(), Guid.NewGuid(), Quantity.Create(qty, "pcs"));

    [Fact]
    public async Task Handle_EmptyDb_ReturnsEmptyList()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var handler = new GetBatchesListQueryHandler(db);

        var response = await handler.Handle(new GetBatchesListQuery(), CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsAllBatches()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        db.Batches.AddRange(MakeBatch("B-001"), MakeBatch("B-002"), MakeBatch("B-003"));
        await db.SaveChangesAsync();

        var handler = new GetBatchesListQueryHandler(db);
        var response = await handler.Handle(new GetBatchesListQuery(), CancellationToken.None);

        response.Items.Should().HaveCount(3);
        response.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_RespectsPagination()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        for (int i = 1; i <= 10; i++) db.Batches.Add(MakeBatch($"B-{i:000}"));
        await db.SaveChangesAsync();

        var handler = new GetBatchesListQueryHandler(db);
        var response = await handler.Handle(new GetBatchesListQuery(Page: 2, PageSize: 3), CancellationToken.None);

        response.Items.Should().HaveCount(3);
        response.TotalCount.Should().Be(10);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task Handle_OrdersByCreatedAtDescending()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var older = MakeBatch("B-001");
        var newer = MakeBatch("B-002");
        db.Batches.AddRange(older, newer);
        await db.SaveChangesAsync();

        var handler = new GetBatchesListQueryHandler(db);
        var response = await handler.Handle(new GetBatchesListQuery(), CancellationToken.None);

        response.Items.First().BatchNumber.Should().Be("B-002");
    }

    [Fact]
    public async Task Handle_ClampsPageSize()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        for (int i = 1; i <= 5; i++) db.Batches.Add(MakeBatch($"B-{i:000}"));
        await db.SaveChangesAsync();

        var handler = new GetBatchesListQueryHandler(db);
        var response = await handler.Handle(new GetBatchesListQuery(Page: 1, PageSize: 9999), CancellationToken.None);

        response.PageSize.Should().Be(100); // clamped
    }
}