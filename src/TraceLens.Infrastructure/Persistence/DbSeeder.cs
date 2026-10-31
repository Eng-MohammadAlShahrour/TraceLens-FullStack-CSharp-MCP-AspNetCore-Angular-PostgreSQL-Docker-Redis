using Microsoft.EntityFrameworkCore;
using TraceLens.Domain.Entities;
using TraceLens.Domain.Enums;
using TraceLens.Domain.ValueObjects;

namespace TraceLens.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Work Orders
        if (!await context.WorkOrders.AnyAsync())
        {
            var wo1 = WorkOrder.Create("WO-2026-001", Guid.Parse("22222222-2222-2222-2222-222222222222"), 100, DateTime.UtcNow.AddDays(30));
            var wo2 = WorkOrder.Create("WO-2026-002", Guid.Parse("44444444-4444-4444-4444-444444444444"), 250, DateTime.UtcNow.AddDays(45));
            var wo3 = WorkOrder.Create("WO-2026-003", Guid.Parse("66666666-6666-6666-6666-666666666666"), 80, DateTime.UtcNow.AddDays(15));
            var wo4 = WorkOrder.Create("WO-2026-004", Guid.Parse("88888888-8888-8888-8888-888888888888"), 150, DateTime.UtcNow.AddDays(60));

            context.WorkOrders.AddRange(wo1, wo2, wo3, wo4);
            await context.SaveChangesAsync();
        }

        // Batches
        if (await context.Batches.AnyAsync())
            return;

        var batch1 = Batch.Create(
            BatchNumber.Create("B-2026-001"),
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Quantity.Create(100, "pcs"));
        batch1.StartProduction();
        batch1.MarkQCCompleted(QualityCheckResult.Passed);
        batch1.ClearDomainEvents();

        var batch2 = Batch.Create(
            BatchNumber.Create("B-2026-002"),
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Quantity.Create(250, "pcs"));
        batch2.StartProduction();
        batch2.ClearDomainEvents();

        var batch3 = Batch.Create(
            BatchNumber.Create("B-2026-003"),
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Quantity.Create(80, "pcs"));
        batch3.StartProduction();
        batch3.MarkQCCompleted(QualityCheckResult.Failed);
        batch3.Quarantine("Dimension out of tolerance");
        batch3.ClearDomainEvents();

        var batch4 = Batch.Create(
            BatchNumber.Create("B-2026-004"),
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Guid.Parse("88888888-8888-8888-8888-888888888888"),
            Quantity.Create(150, "pcs"));
        batch4.ClearDomainEvents();

        context.Batches.AddRange(batch1, batch2, batch3, batch4);
        await context.SaveChangesAsync();
    }
}
