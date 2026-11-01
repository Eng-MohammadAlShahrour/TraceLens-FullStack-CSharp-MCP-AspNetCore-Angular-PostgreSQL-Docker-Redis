using Microsoft.EntityFrameworkCore;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Domain.Enums;

namespace TraceLens.API.Jobs;

public sealed class CheckStaleBatchesJob
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CheckStaleBatchesJob> _logger;

    public CheckStaleBatchesJob(IApplicationDbContext context, ILogger<CheckStaleBatchesJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Run(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        var stale = await _context.Batches
            .Where(b => b.Status == BatchStatus.InProduction
                        && b.StartedAt.HasValue
                        && b.StartedAt.Value < cutoff)
            .ToListAsync(ct);

        foreach (var batch in stale)
        {
            _logger.LogWarning("STALE BATCH: {BatchNumber} in production since {StartedAt}",
                batch.BatchNumber.Value, batch.StartedAt);
        }

        _logger.LogInformation("CheckStaleBatchesJob: found {Count} stale batches", stale.Count);
    }
}
