// Infrastructure/Data/Configurations/ProductConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Table name
            builder.ToTable("Products");

            // Primary key
            builder.HasKey(p => p.Id);

            // Properties configuration
            builder.Property(p => p.Id)
                .ValueGeneratedNever(); // Guid is generated in domain

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.Unit)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.Category);
            
            // Unique constraints
            builder.HasIndex(p => p.Name)
                .IsUnique();

            // Relationships
            builder.HasMany(p => p.StockItems)
                .WithOne(si => si.Product)
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if stock items exist

            builder.HasMany(p => p.StockTransactions)
                .WithOne(st => st.Product)
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if transactions exist
        }
    }
}