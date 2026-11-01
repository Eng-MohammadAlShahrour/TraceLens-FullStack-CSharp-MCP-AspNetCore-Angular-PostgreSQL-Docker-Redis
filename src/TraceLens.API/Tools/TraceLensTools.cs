using System.ComponentModel;
using System.Text.Json;
using MediatR;
using ModelContextProtocol.Server;
using TraceLens.Application.Features.Batches.CreateBatch;
using TraceLens.Application.Features.Batches.GetBatchesList;
using TraceLens.Application.Features.WorkOrders.GetWorkOrdersList;

namespace TraceLens.API.Tools;

[McpServerToolType]
public static class TraceLensTools
{
    [McpServerTool, Description("Get batches filtered by status (Created, InProduction, QCPassed, QCFailed, Quarantined, Shipped).")]
    public static async Task<string> GetBatchesByStatus(
        [Description("Status e.g. InProduction")] string status,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetBatchesListQuery(1, 100), ct);
        var filtered = result.Items
            .Where(b => string.Equals(b.Status, status, StringComparison.OrdinalIgnoreCase))
            .Select(b => new { b.BatchNumber, b.Status, b.ProducedQuantity, b.Unit, b.CreatedAt })
            .ToList();
        return JsonSerializer.Serialize(new { status, total = filtered.Count, batches = filtered },
            new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get all production work orders with planned quantity and due date.")]
    public static async Task<string> GetWorkOrders(
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetWorkOrdersListQuery(1, 100), ct);
        var orders = result.Items
            .Select(w => new { w.OrderNumber, w.PlannedQuantity, w.DueDate, w.IsClosed, w.CreatedAt })
            .ToList();
        return JsonSerializer.Serialize(new { total = orders.Count, workOrders = orders },
            new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Create a new production batch.")]
    public static async Task<string> CreateBatch(
        [Description("Unique batch number, e.g. B-2026-010")] string batchNumber,
        [Description("Work order ID (GUID)")] string workOrderId,
        [Description("Product ID (GUID)")] string productId,
        [Description("Produced quantity")] decimal producedQuantity,
        [Description("Unit, e.g. pcs")] string producedQuantityUnit,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new CreateBatchCommand(
            batchNumber,
            Guid.Parse(workOrderId),
            Guid.Parse(productId),
            producedQuantity,
            producedQuantityUnit), ct);

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
}
