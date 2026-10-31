using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraceLens.Domain.Entities;
using TraceLens.Domain.ValueObjects;

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

        builder.Property(m => m.Quantity)
            .HasConversion(
                v => v.Value,
                v => Quantity.Create(v, "unit"))
            .HasColumnName("quantity")
            .HasPrecision(18, 4);

        builder.Property(m => m.ReceivedAt).HasColumnName("received_at");
        builder.Property(m => m.CertificateOfAnalysis).HasColumnName("certificate_of_analysis").HasMaxLength(2000);
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");
    }
}
