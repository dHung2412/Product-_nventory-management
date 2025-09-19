// Infrastructure/Data/Configurations/WarehouseConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            // Table name
            builder.ToTable("Warehouses");

            // Primary key
            builder.HasKey(w => w.Id);

            // Properties configuration
            builder.Property(w => w.Id)
                .ValueGeneratedNever(); // Guid is generated in domain

            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(w => w.Address)
                .IsRequired()
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(w => w.Name);
            
            // Unique constraints
            builder.HasIndex(w => w.Name)
                .IsUnique();

            // Relationships
            builder.HasMany<StockItem>()
                .WithOne(si => si.Warehouse)
                .HasForeignKey(si => si.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if stock items exist

            builder.HasMany<StockTransaction>()
                .WithOne(st => st.Warehouse)
                .HasForeignKey(st => st.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if transactions exist
        }
    }
}