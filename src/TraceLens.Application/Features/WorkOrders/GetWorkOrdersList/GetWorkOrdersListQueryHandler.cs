using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Application.Features.WorkOrders.Common;

namespace TraceLens.Application.Features.WorkOrders.GetWorkOrdersList;

public sealed class GetWorkOrdersListQueryHandler
    : IRequestHandler<GetWorkOrdersListQuery, GetWorkOrdersListResponse>
{
    private readonly IApplicationDbContext _context;

    public GetWorkOrdersListQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<GetWorkOrdersListResponse> Handle(GetWorkOrdersListQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _context.WorkOrders
            .AsNoTracking()
            .OrderByDescending(w => w.CreatedAt);

        var total = await query.CountAsync(ct);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities
            .Select(w => new WorkOrderDto(
                w.Id,
                w.OrderNumber,
                w.ProductId,
                w.PlannedQuantity,
                w.DueDate,
                w.IsClosed,
                w.CreatedAt))
            .ToList();

        return new GetWorkOrdersListResponse(items, total, page, pageSize);
    }
}
