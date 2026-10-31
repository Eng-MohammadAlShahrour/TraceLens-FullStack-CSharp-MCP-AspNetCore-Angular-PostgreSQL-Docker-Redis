namespace TraceLens.Application.Features.WorkOrders.Common;

public sealed record WorkOrderDto(
    Guid Id,
    string OrderNumber,
    Guid ProductId,
    int PlannedQuantity,
    DateTime DueDate,
    bool IsClosed,
    DateTime CreatedAt
);
