using System.ComponentModel;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TraceLens.Application.Features.Batches.CreateBatch;
using TraceLens.Application.Features.Batches.GetBatchesList;
using TraceLens.Application.Features.WorkOrders.GetWorkOrdersList;

namespace TraceLens.API.Tools;

[McpServerToolType]
public static class TraceLensTools
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    [McpServerTool, Description("Get batches filtered by status (Created, InProduction, QCPassed, QCFailed, Quarantined, Shipped).")]
    public static async Task<string> GetBatchesByStatus(
        [Description("Status e.g. InProduction")] string status,
        ISender mediator,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetBatchesListQuery(1, 100), ct);
            var filtered = result.Items
                .Where(b => string.Equals(b.Status, status, StringComparison.OrdinalIgnoreCase))
                .Select(b => new { b.BatchNumber, b.Status, b.ProducedQuantity, b.Unit, b.CreatedAt })
                .ToList();
            return JsonSerializer.Serialize(new { status, total = filtered.Count, batches = filtered }, JsonOpts);
        }
        catch (Exception ex)
        {
            throw ToMcpException(ex);
        }
    }

    [McpServerTool, Description("Get all production work orders with planned quantity and due date.")]
    public static async Task<string> GetWorkOrders(
        ISender mediator,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetWorkOrdersListQuery(1, 100), ct);
            var orders = result.Items
                .Select(w => new { w.OrderNumber, w.PlannedQuantity, w.DueDate, w.IsClosed, w.CreatedAt })
                .ToList();
            return JsonSerializer.Serialize(new { total = orders.Count, workOrders = orders }, JsonOpts);
        }
        catch (Exception ex)
        {
            throw ToMcpException(ex);
        }
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
        try
        {
            if (!Guid.TryParse(workOrderId, out var woGuid))
                throw new McpException($"workOrderId '{workOrderId}' is not a valid GUID.");
            if (!Guid.TryParse(productId, out var prodGuid))
                throw new McpException($"productId '{productId}' is not a valid GUID.");

            var result = await mediator.Send(new CreateBatchCommand(
                batchNumber, woGuid, prodGuid, producedQuantity, producedQuantityUnit), ct);

            return JsonSerializer.Serialize(result, JsonOpts);
        }
        catch (Exception ex)
        {
            throw ToMcpException(ex);
        }
    }

    private static McpException ToMcpException(Exception ex)
    {
        if (ex is McpException mcp) return mcp;

        if (ex is ValidationException ve)
        {
            var msgs = ve.Errors?
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();
            var text = (msgs is { Count: > 0 }) ? string.Join(" | ", msgs!) : "Validation failed.";
            return new McpException(text);
        }

        if (ex is InvalidOperationException || ex is ArgumentException)
            return new McpException(ex.Message);

        if (ex is DbUpdateException dbEx)
        {
            var inner = dbEx.InnerException?.Message ?? dbEx.Message;
            if (inner.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
                || inner.Contains("unique constraint", StringComparison.OrdinalIgnoreCase)
                || inner.Contains("IX_batches_batch_number", StringComparison.OrdinalIgnoreCase))
            {
                return new McpException("A batch with the same number already exists. Please choose a different batch number.");
            }
            return new McpException("Database error occurred while saving the batch.");
        }

        if (ex.InnerException is not null && ex.InnerException != ex)
            return ToMcpException(ex.InnerException);

        return new McpException(ex.Message);
    }
}
