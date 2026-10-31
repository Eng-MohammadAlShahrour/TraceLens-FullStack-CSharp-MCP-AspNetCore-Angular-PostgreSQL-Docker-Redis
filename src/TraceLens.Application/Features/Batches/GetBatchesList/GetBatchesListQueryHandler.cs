using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Interfaces;

namespace TraceLens.Application.Features.Batches.GetBatchesList;

public sealed class GetBatchesListQueryHandler
    : IRequestHandler<GetBatchesListQuery, GetBatchesListResponse>
{
    private readonly IApplicationDbContext _context;

    public GetBatchesListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetBatchesListResponse> Handle(
        GetBatchesListQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _context.Batches
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt);

        var total = await query.CountAsync(cancellationToken);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = entities
            .Select(b => new BatchListItemDto(
                b.Id,
                b.BatchNumber.Value,
                b.Status.ToString(),
                b.ProducedQuantity.Value,
                b.ProducedQuantity.Unit,
                b.CreatedAt))
            .ToList();

        return new GetBatchesListResponse(items, total, page, pageSize);
    }
}
