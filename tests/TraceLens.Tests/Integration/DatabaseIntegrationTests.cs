using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TraceLens.Domain.Entities;
using TraceLens.Domain.Enums;
using TraceLens.Domain.ValueObjects;
using TraceLens.Infrastructure.Persistence;

namespace TraceLens.Tests.Integration;

public class DatabaseIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public DatabaseIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = new ApplicationDbContext(_options);
        ctx.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private ApplicationDbContext NewContext() => new(_options);

    [Fact]
    public async Task Batch_WithOwnedQuantity_PersistsValueAndUnit()
    {
        var id = Guid.NewGuid();
        using (var ctx = NewContext())
        {
            var batch = Batch.Create(BatchNumber.Create("B-001"), Guid.NewGuid(), Guid.NewGuid(), Quantity.Create(250.5m, "kg"));
            batch.ClearDomainEvents();
            ctx.Batches.Add(batch);
            await ctx.SaveChangesAsync();
            id = batch.Id;
        }

        using (var ctx = NewContext())
        {
            var loaded = await ctx.Batches.SingleAsync(b => b.Id == id);
            loaded.ProducedQuantity.Value.Should().Be(250.5m);
            loaded.ProducedQuantity.Unit.Should().Be("kg");
        }
    }

    [Fact]
    public async Task Batch_StatusEnum_StoredAsString()
    {
        using (var ctx = NewContext())
        {
            var b = Batch.Create(BatchNumber.Create("B-002"), Guid.NewGuid(), Guid.NewGuid(), Quantity.Create(1, "pcs"));
            b.StartProduction();
            b.MarkQCCompleted(QualityCheckResult.Passed);
            b.ClearDomainEvents();
            ctx.Batches.Add(b);
            await ctx.SaveChangesAsync();
        }

        using (var conn = new SqliteConnection(_connection.ConnectionString))
        {
            // read raw column to verify it's TEXT
            using var ctx = NewContext();
            var row = await ctx.Database
                .SqlQueryRaw<string>("SELECT status FROM batches LIMIT 1")
                .ToListAsync();
            row.Should().ContainSingle();
            row[0].Should().Be("QCPassed");
        }
    }

    [Fact]
    public async Task Batch_BatchNumber_UniqueIndex_Enforced()
    {
        using (var ctx = NewContext())
        {
            var b1 = Batch.Create(BatchNumber.Create("B-UNIQUE"), Guid.NewGuid(), Guid.NewGuid(), Quantity.Create(1, "pcs"));
            b1.ClearDomainEvents();
            ctx.Batches.Add(b1);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = NewContext())
        {
            var b2 = Batch.Create(BatchNumber.Create("B-UNIQUE"), Guid.NewGuid(), Guid.NewGuid(), Quantity.Create(2, "pcs"));
            b2.ClearDomainEvents();
            ctx.Batches.Add(b2);
            var act = async () => await ctx.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }

    [Fact]
    public async Task MaterialLot_WithOwnedQuantity_RoundTrips()
    {
        var batchId = Guid.NewGuid();
        using (var ctx = NewContext())
        {
            var batch = Batch.Create(BatchNumber.Create("B-003"), Guid.NewGuid(), Guid.NewGuid(), Quantity.Create(10, "pcs"));
            var lot = MaterialLot.Create("LOT-001", Guid.NewGuid(), MaterialType.Raw, Quantity.Create(50.25m, "kg"), "CoA-123");
            batch.AddMaterialLot(lot);
            batch.ClearDomainEvents();
            ctx.Batches.Add(batch);
            await ctx.SaveChangesAsync();
            batchId = batch.Id;
        }

        using (var ctx = NewContext())
        {
            var batch = await ctx.Batches.Include(b => b.MaterialLots).SingleAsync(b => b.Id == batchId);
            batch.MaterialLots.Should().ContainSingle();
            var lot = batch.MaterialLots.First();
            lot.LotNumber.Should().Be("LOT-001");
            lot.Quantity.Value.Should().Be(50.25m);
            lot.Quantity.Unit.Should().Be("kg");
            lot.CertificateOfAnalysis.Should().Be("CoA-123");
        }
    }
}