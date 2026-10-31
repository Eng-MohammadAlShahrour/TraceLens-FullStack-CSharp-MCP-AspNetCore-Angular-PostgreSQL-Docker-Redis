using TraceLens.Application.Features.WorkOrders.Common;

namespace TraceLens.Application.Features.WorkOrders.GetWorkOrdersList;

public sealed record GetWorkOrdersListResponse(
    List<WorkOrderDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
