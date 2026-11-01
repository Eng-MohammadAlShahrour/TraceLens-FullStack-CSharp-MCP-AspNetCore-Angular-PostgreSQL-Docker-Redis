using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TraceLens.Application.Common.Interfaces;

namespace TraceLens.Application.Features.Batches.GetBatchesList;

public sealed class GetBatchesListQueryHandler
    : IRequestHandler<GetBatchesListQuery, GetBatchesListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);
    private static readonly JsonSerializerOptions JsonOpts = new();

    public GetBatchesListQueryHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<GetBatchesListResponse> Handle(
        GetBatchesListQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var cacheKey = $"batches:list:p{page}:s{pageSize}";

        // Try cache (best-effort — never let Redis break the API)
        try
        {
            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cached))
            {
                var hit = JsonSerializer.Deserialize<GetBatchesListResponse>(cached, JsonOpts);
                if (hit is not null) return hit;
            }
        }
        catch { /* Redis down — fall through to DB */ }

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

        var response = new GetBatchesListResponse(items, total, page, pageSize);

        // Write cache (best-effort)
        try
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response, JsonOpts),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl },
                cancellationToken);
        }
        catch { /* Redis down — ignore */ }

        return response;
    }
}
