using MediatR;

namespace TraceLens.Application.Features.WorkOrders.GetWorkOrdersList;

public sealed record GetWorkOrdersListQuery(int Page = 1, int PageSize = 20)
    : IRequest<GetWorkOrdersListResponse>;
