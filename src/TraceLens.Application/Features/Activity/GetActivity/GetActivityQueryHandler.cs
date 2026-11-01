using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Interfaces;

namespace TraceLens.Application.Features.Activity.GetActivity;

public sealed class GetActivityQueryHandler
    : IRequestHandler<GetActivityQuery, GetActivityResponse>
{
    private readonly IApplicationDbContext _context;

    public GetActivityQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<GetActivityResponse> Handle(
        GetActivityQuery request,
        CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit, 1, 500);

        var total = await _context.AuditLogs.CountAsync(cancellationToken);

        var entities = await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var items = entities.Select(x => new AuditLogDto(
            x.Id, x.EventType, x.AggregateId, x.BatchNumber,
            x.PreviousStatus, x.NewStatus, x.Reason, x.CreatedAt))
            .ToList();

        return new GetActivityResponse(items, total);
    }
}
