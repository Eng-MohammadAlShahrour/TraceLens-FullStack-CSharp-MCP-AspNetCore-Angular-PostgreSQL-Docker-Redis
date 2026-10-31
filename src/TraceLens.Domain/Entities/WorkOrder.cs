using TraceLens.Domain.Common;

namespace TraceLens.Domain.Entities;

public sealed class WorkOrder : AggregateRoot
{
    public string OrderNumber { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public int PlannedQuantity { get; private set; }
    public DateTime DueDate { get; private set; }
    public bool IsClosed { get; private set; }

    private WorkOrder() { }

    public static WorkOrder Create(string orderNumber, Guid productId, int plannedQuantity, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number is required.", nameof(orderNumber));
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.", nameof(productId));
        if (plannedQuantity <= 0)
            throw new ArgumentException("Planned quantity must be positive.", nameof(plannedQuantity));
        if (dueDate < DateTime.UtcNow.Date)
            throw new ArgumentException("Due date must be in the future.", nameof(dueDate));

        return new WorkOrder
        {
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            ProductId = productId,
            PlannedQuantity = plannedQuantity,
            DueDate = dueDate
        };
    }

    public void Close()
    {
        if (IsClosed)
            throw new InvalidOperationException("Work order is already closed.");
        IsClosed = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
