using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceLens.Domain.Entities;

namespace TraceLens.Infrastructure.Persistence.Configurations;

public sealed class MaterialLotConfiguration : IEntityTypeConfiguration<MaterialLot>
{
    public void Configure(EntityTypeBuilder<MaterialLot> builder)
    {
        builder.ToTable("material_lots");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.LotNumber)
            .HasColumnName("lot_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.SupplierId).HasColumnName("supplier_id");

        builder.Property(m => m.Type)
            .HasConversion<string>()
            .HasColumnName("type")
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(m => m.Quantity, q =>
        {
            q.Property(x => x.Value)
                .HasColumnName("quantity_value")
                .HasPrecision(18, 4)
                .IsRequired();
            q.Property(x => x.Unit)
                .HasColumnName("quantity_unit")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Property(m => m.ReceivedAt).HasColumnName("received_at");
        builder.Property(m => m.CertificateOfAnalysis).HasColumnName("certificate_of_analysis").HasMaxLength(2000);
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");
    }
}