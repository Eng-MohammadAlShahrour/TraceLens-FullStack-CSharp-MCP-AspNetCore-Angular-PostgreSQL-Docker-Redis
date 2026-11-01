using Microsoft.EntityFrameworkCore;
using TraceLens.Domain.Entities;

namespace TraceLens.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Batch> Batches { get; }
    DbSet<MaterialLot> MaterialLots { get; }
    DbSet<WorkOrder> WorkOrders { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
