// Infrastructure/Data/Configurations/StockItemConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> builder)
        {
            // Table name
            builder.ToTable("StockItems");

            // Primary key
            builder.HasKey(si => si.Id);

            // Properties configuration
            builder.Property(si => si.Id)
                .ValueGeneratedNever(); // Guid is generated in domain

            builder.Property(si => si.ProductId)
                .IsRequired();

            builder.Property(si => si.WarehouseId)
                .IsRequired();

            builder.Property(si => si.Quantity)
                .IsRequired();

            // Indexes
            builder.HasIndex(si => si.ProductId);
            builder.HasIndex(si => si.WarehouseId);
            builder.HasIndex(si => si.Quantity);

            // Composite unique constraint - one product per warehouse
            builder.HasIndex(si => new { si.ProductId, si.WarehouseId })
                .IsUnique();

            // Relationships
            builder.HasOne(si => si.Product)
                .WithMany(p => p.StockItems)
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(si => si.Warehouse)
                .WithMany()
                .HasForeignKey(si => si.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}