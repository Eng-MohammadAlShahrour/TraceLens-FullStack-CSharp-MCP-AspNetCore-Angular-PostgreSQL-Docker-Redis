using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceLens.Domain.Entities;

namespace TraceLens.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.AggregateId).HasColumnName("aggregate_id").IsRequired();
        builder.Property(x => x.AggregateType).HasColumnName("aggregate_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.BatchNumber).HasColumnName("batch_number").HasMaxLength(50).IsRequired();
        builder.Property(x => x.PreviousStatus).HasColumnName("previous_status").HasMaxLength(50);
        builder.Property(x => x.NewStatus).HasColumnName("new_status").HasMaxLength(50);
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_audit_logs_created_at");
        builder.HasIndex(x => x.AggregateId).HasDatabaseName("ix_audit_logs_aggregate_id");
        builder.HasIndex(x => x.EventType).HasDatabaseName("ix_audit_logs_event_type");
    }
}
