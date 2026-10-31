namespace TraceLens.Application.Features.WorkOrders.CreateWorkOrder;

public sealed record CreateWorkOrderResponse(
    Guid Id,
    string OrderNumber,
    int PlannedQuantity,
    DateTime DueDate,
    bool IsClosed,
    DateTime CreatedAt
);
