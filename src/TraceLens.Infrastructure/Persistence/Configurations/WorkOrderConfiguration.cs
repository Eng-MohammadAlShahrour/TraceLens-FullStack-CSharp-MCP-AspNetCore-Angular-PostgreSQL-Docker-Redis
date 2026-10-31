using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceLens.Domain.Entities;

namespace TraceLens.Infrastructure.Persistence.Configurations;

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("work_orders");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.OrderNumber)
            .HasColumnName("order_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(w => w.OrderNumber).IsUnique();

        builder.Property(w => w.ProductId).HasColumnName("product_id");
        builder.Property(w => w.PlannedQuantity).HasColumnName("planned_quantity");
        builder.Property(w => w.DueDate).HasColumnName("due_date");
        builder.Property(w => w.IsClosed).HasColumnName("is_closed");
        builder.Property(w => w.CreatedAt).HasColumnName("created_at");
        builder.Property(w => w.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(w => w.DomainEvents);
    }
}
