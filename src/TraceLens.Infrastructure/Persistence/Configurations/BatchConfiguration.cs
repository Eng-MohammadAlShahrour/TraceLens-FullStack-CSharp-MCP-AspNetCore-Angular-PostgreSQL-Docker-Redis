using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceLens.Domain.Entities;
using TraceLens.Domain.ValueObjects;

namespace TraceLens.Infrastructure.Persistence.Configurations;

public sealed class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("batches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BatchNumber)
            .HasConversion(
                v => v.Value,
                v => BatchNumber.Create(v))
            .HasColumnName("batch_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(b => b.BatchNumber).IsUnique();

        builder.Property(b => b.ProducedQuantity)
            .HasConversion(
                v => v.Value,
                v => Quantity.Create(v, "unit"))
            .HasColumnName("produced_quantity")
            .HasPrecision(18, 4);

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(b => b.Notes).HasMaxLength(1000);

        builder.Property(b => b.CreatedAt).HasColumnName("created_at");
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at");
        builder.Property(b => b.StartedAt).HasColumnName("started_at");
        builder.Property(b => b.CompletedAt).HasColumnName("completed_at");
        builder.Property(b => b.WorkOrderId).HasColumnName("work_order_id");
        builder.Property(b => b.ProductId).HasColumnName("product_id");

        builder.HasMany(b => b.MaterialLots)
            .WithOne()
            .HasForeignKey("batch_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.MaterialLots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(b => b.DomainEvents);
    }
}
