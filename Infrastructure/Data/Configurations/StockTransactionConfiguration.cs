// Infrastructure/Data/Configurations/StockTransactionConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
    {
        public void Configure(EntityTypeBuilder<StockTransaction> builder)
        {
            // Table name
            builder.ToTable("StockTransactions");

            // Primary key
            builder.HasKey(st => st.Id);

            // Properties configuration
            builder.Property(st => st.Id)
                .ValueGeneratedNever(); // Guid is generated in domain

            builder.Property(st => st.ProductId)
                .IsRequired();

            builder.Property(st => st.WarehouseId)
                .IsRequired();

            builder.Property(st => st.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(st => st.Quantity)
                .IsRequired();

            builder.Property(st => st.Reason)
                .HasMaxLength(500);

            builder.Property(st => st.TransactionDate)
                .IsRequired();

            builder.Property(st => st.UserId)
                .IsRequired();

            // Indexes
            builder.HasIndex(st => st.ProductId);
            builder.HasIndex(st => st.WarehouseId);
            builder.HasIndex(st => st.UserId);
            builder.HasIndex(st => st.Type);
            builder.HasIndex(st => st.TransactionDate);

            // Composite indexes for better query performance
            builder.HasIndex(st => new { st.ProductId, st.TransactionDate });
            builder.HasIndex(st => new { st.WarehouseId, st.TransactionDate });
            builder.HasIndex(st => new { st.Type, st.TransactionDate });

            // Relationships
            builder.HasOne(st => st.Product)
                .WithMany(p => p.StockTransactions)
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(st => st.Warehouse)
                .WithMany()
                .HasForeignKey(st => st.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(st => st.User)
                .WithMany(u => u.StockTransactions)
                .HasForeignKey(st => st.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}