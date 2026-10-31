using MediatR;

namespace TraceLens.Application.Features.WorkOrders.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(
    string OrderNumber,
    Guid ProductId,
    int PlannedQuantity,
    DateTime DueDate
) : IRequest<CreateWorkOrderResponse>;
