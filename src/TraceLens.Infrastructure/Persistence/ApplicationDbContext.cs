using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Domain.Common;
using TraceLens.Domain.Entities;

namespace TraceLens.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<MaterialLot> MaterialLots => Set<MaterialLot>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseOpenIddict();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                // UpdatedAt is protected
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
